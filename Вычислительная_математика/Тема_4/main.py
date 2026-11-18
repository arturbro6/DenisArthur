#!/usr/bin/env python3
"""Тема 4. Python 3, только стандартная библиотека.
Запуск: python3 main.py --all либо python3 main.py (интерактивное меню).
"""
import argparse
import csv
import json
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parent


def solve(f, a, b, initial, n, method="rk4", sample_every=1):
    """Явный Эйлер или классический РК4 для векторной задачи Коши.
    Все компоненты шага вычисляются одновременно по старому вектору.
    """
    if n < 1 or sample_every < 1 or b <= a:
        raise ValueError("Некорректная сетка")
    if method not in ("euler", "rk4"):
        raise ValueError("Неизвестный метод")
    h = (b - a) / n
    u = tuple(initial)
    rows = [(a, *u)]
    for i in range(n):
        t = a + i * h
        k1 = f(t, u)
        if method == "euler":
            u = tuple(v + h * k for v, k in zip(u, k1))
        else:
            k2 = f(t + h / 2, tuple(v + h * k / 2 for v, k in zip(u, k1)))
            k3 = f(t + h / 2, tuple(v + h * k / 2 for v, k in zip(u, k2)))
            k4 = f(t + h, tuple(v + h * k for v, k in zip(u, k3)))
            u = tuple(v + h * (p + 2*q + 2*r + s) / 6
                      for v, p, q, r, s in zip(u, k1, k2, k3, k4))
        if (i + 1) % sample_every == 0 or i + 1 == n:
            rows.append((a + (i + 1) * h, *u))
    return rows


def f1(x, u):
    return (u[0] * (1 - x),)


def f2(x, u):
    y, v = u
    return (v, -v / x - y)


def f3(t, u):
    x, y, z = u
    return (-2*x + 5*z, math.sin(t - 1)*x - y + 3*z, -x + 2*z)


PROBLEMS = {
    1: (f1, 0., 1., (1.,), ("x", "y")),
    2: (f2, 1., 1.5, (.77, -.44), ("x", "y", "v")),
    3: (f3, 0., .3, (2., 1., 1.), ("t", "x", "y", "z")),
}


def run(problem, n, method, sample_every=1):
    f, a, b, initial, _ = PROBLEMS[problem]
    return solve(f, a, b, initial, n, method, sample_every)


def discrepancy(a, b):
    assert len(a) == len(b)
    return max(abs(x-y) for r, s in zip(a, b) for x, y in zip(r[1:], s[1:]))


def refine(problem, base_n=10, tolerance=1e-5):
    reference = run(problem, base_n, "rk4")
    history = []
    factor = 1
    while True:
        rows = run(problem, base_n * factor, "euler", factor)
        error = discrepancy(rows, reference)
        history.append({"n": base_n*factor,
                        "h": (PROBLEMS[problem][2]-PROBLEMS[problem][1])/(base_n*factor),
                        "difference": error})
        if error <= tolerance:
            return rows, history
        factor *= 2
        if base_n * factor > 2_000_000:
            raise RuntimeError("Не достигнут заданный допуск")


def save_csv(path, headers, rows):
    with path.open("w", newline="", encoding="utf-8-sig") as file:
        writer = csv.writer(file)
        writer.writerow(headers)
        writer.writerows(rows)


def graph(path, series, labels, title):
    """Самостоятельный SVG-график без внешних библиотек."""
    from html import escape
    values = [(r[0], r[1]) for rows in series for r in rows]
    lo, hi = min(y for x,y in values), max(y for x,y in values)
    xmin, xmax = min(x for x,y in values), max(x for x,y in values)
    pad = max((hi-lo)*.08, .001)
    lo, hi = lo-pad, hi+pad
    colors = ["#db622a", "#2563a6", "#249066", "#9747aa"]
    chunks = ['<svg xmlns="http://www.w3.org/2000/svg" width="900" height="470" viewBox="0 0 900 470">',
              '<rect width="900" height="470" fill="white"/>',
              f'<text x="65" y="30" font-size="20" font-family="Arial">{escape(title)}</text>']
    for j in range(6):
        y = 65+j*60
        value = hi-(hi-lo)*j/5
        chunks.append(f'<path d="M80 {y}H850" stroke="#ddd"/><text x="8" y="{y+5}" font-size="13">{value:.5f}</text>')
        x = 80+j*154
        chunks.append(f'<text x="{x-15}" y="390" font-size="13">{xmin+(xmax-xmin)*j/5:.3f}</text>')
    for i,(rows,label) in enumerate(zip(series,labels)):
        pts = ' '.join(f'{80+(x-xmin)/(xmax-xmin)*770:.2f},{65+(hi-y)/(hi-lo)*300:.2f}' for x,y,*_ in rows)
        c=colors[i%len(colors)]
        chunks.append(f'<polyline points="{pts}" fill="none" stroke="{c}" stroke-width="2"/>')
        chunks.append(f'<text x="{80+i*200}" y="435" font-size="14" fill="{c}" font-family="Arial">{escape(label)}</text>')
    path.write_text('\n'.join(chunks)+"</svg>",encoding="utf-8")


def all_results():
    out = ROOT / "results"
    out.mkdir(exist_ok=True)
    result = {"tolerance": 1e-5}
    for p in (1, 2):
        e = run(p, 10, "euler")
        r = run(p, 10, "rk4")
        refined, history = refine(p)
        reference = run(p, 10000, "rk4", 1000)
        result[str(p)] = {"euler": e, "rk4": r, "refined": refined,
                          "history": history, "reference": reference,
                          "rk4_error": discrepancy(r, reference)}
        for label,rows in (("euler_n10",e),("rk4_n10",r),("euler_refined",refined)):
            save_csv(out/f"example{p}_{label}.csv",PROBLEMS[p][4],rows)
        save_csv(out/f"example{p}_convergence.csv",("n","h","difference"),
                 [(v['n'],v['h'],v['difference']) for v in history])
        graph(out/f"example{p}.svg",[e,r,refined],["Эйлер, n=10","Рунге-Кутта 4","Эйлер, уточнение"],f"Пример {p}: y(x)")
    # Дополнительно рассчитываем явно указанный h=0.1 во втором примере.
    result["2"]["h01_euler"] = run(2,5,"euler")
    result["2"]["h01_rk4"] = run(2,5,"rk4")
    for m in ("euler","rk4"):
        save_csv(out/f"example2_{m}_h01.csv",PROBLEMS[2][4],result['2'][f'h01_{m}'])
    result['3'] = {}
    # n=10 соответствует h=.03, буквальный h=.003 — n=100;
    # каждый шаг дополнительно уменьшаем в 10 раз, до .003 и .0003.
    for n in (10,100,1000):
        rows=run(3,n,"euler")
        result['3'][str(n)]=rows
        save_csv(out/f"example3_euler_n{n}.csv",PROBLEMS[3][4],rows)
    ref=run(3,10000,"rk4",1000)
    result['3']['reference']=ref
    for n in (10,100,1000):
        result['3'][f'error{n}']=discrepancy(run(3,n,"euler",n//10),ref)
    rows=result['3']['1000']
    graph(out/'example3.svg', [[(r[0],r[k]) for r in rows] for k in (1,2,3)],
          ['x(t)','y(t)','z(t)'],'Пример 3: система, Эйлер h=0.0003')
    (out/'results.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
    print("Все расчёты завершены. Таблицы CSV, графики SVG и results.json: " + str(out))
    for p in (1,2):
        d=result[str(p)]
        print(f"Пример {p}: Эйлер y(b)={d['euler'][-1][1]:.9f}; РК4 y(b)={d['rk4'][-1][1]:.9f}; уточнение n={d['history'][-1]['n']}")
    for n in (10,100,1000):
        print(f"Система n={n}: " + ', '.join(f'{v:.9f}' for v in result['3'][str(n)][-1][1:]))
    return result


def menu():
    while True:
        print("\nВычислительная математика\n1. Дифференциальные уравнения\n2. Рассчитать всю лабораторную\n0. Выход")
        choice=input("> ").strip()
        if choice=='0': return
        if choice=='2': all_results(); continue
        if choice!='1': print("Выберите 0, 1 или 2"); continue
        method=input("1. Метод Эйлера\n2. Метод Рунге-Кутта 4\n> ").strip()
        if method not in ('1','2'): print("Неверный метод"); continue
        try:
            p=int(input("Номер примера (1, 2, 3): "))
            if p not in PROBLEMS: raise ValueError()
            n=int(input("Количество частей (например, 10): "))
            if n<1 or n>2_000_000: raise ValueError()
            rows=run(p,n,'euler' if method=='1' else 'rk4')
        except ValueError:
            print("Введите допустимые целые числа"); continue
        print(' '.join(f'{s:>14}' for s in PROBLEMS[p][4]))
        # Для больших сеток консоль показывает первые и последние строки;
        # полный результат всегда сохраняется в CSV.
        visible = rows if len(rows)<=50 else rows[:20]+rows[-20:]
        for row in visible: print(' '.join(f'{v:14.9f}' for v in row))
        if len(rows)>50: print("Показаны первые и последние 20 строк; полная таблица в CSV.")
        path=ROOT/'results'/'interactive.csv'
        path.parent.mkdir(exist_ok=True)
        save_csv(path,PROBLEMS[p][4],rows)
        print("Сохранено: "+str(path))


def verify():
    # Аналитическое решение первого примера, проверка точности и порядка.
    exact=math.exp(.5)
    errs=[abs(run(1,n,'euler')[-1][1]-exact) for n in (10,20,40)]
    assert all(1.7<a/b<2.2 for a,b in zip(errs,errs[1:]))
    assert abs(run(1,10,'rk4')[-1][1]-exact)<1e-6
    # Одновременность шага в системе и знак в приведённом ДУ второго порядка.
    assert discrepancy(run(3,1,'euler'),[(0,2,1,1),(.3,2.3,1+.3*(2*math.sin(-1)+2),1)])<1e-12
    assert discrepancy(run(2,1,'euler'),[(1,.77,-.44),(1.5,.55,-.605)])<1e-12
    for p in (2,3):
        assert discrepancy(run(p,100,'rk4',10),run(p,200,'rk4',20))<1e-8
    print("Проверки пройдены: аналитическое решение, сходимость, одновременный шаг системы, эталон РК4.")


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--all',action='store_true')
    parser.add_argument('--check',action='store_true')
    args=parser.parse_args()
    if args.check: verify()
    elif args.all: all_results()
    else: menu()
