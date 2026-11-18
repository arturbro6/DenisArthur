"""Собственные замеры: CSV всегда; matplotlib PNG при --plot."""
import argparse
import csv
import statistics
import timeit
from pathlib import Path
from variants import get_variant
from memo import (gen_bin_tree_recursive, gen_bin_tree_iterative,
                  gen_bin_tree_recursive_memo, gen_bin_tree_iterative_memo,
                  gen_bin_tree_recursive_lru)

FUNCTIONS = [gen_bin_tree_recursive, gen_bin_tree_iterative,
             gen_bin_tree_recursive_memo, gen_bin_tree_iterative_memo,
             gen_bin_tree_recursive_lru]


def clear(f):
    if hasattr(f, "cache_clear"): f.cache_clear()


def measure(f, args, repeat, warm):
    values=[]
    for _ in range(repeat):
        clear(f)
        if warm: f(*args)
        values.append(timeit.Timer(lambda: f(*args)).timeit(number=1))
    return statistics.mean(values)


def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--variant',type=int,default=1)
    p.add_argument('--max-height',type=int,default=12)
    p.add_argument('--repeat',type=int,default=3)
    p.add_argument('--plot',action='store_true')
    a=p.parse_args()
    if a.repeat<1 or not 2<=a.max_height<=18: p.error('repeat >= 1, max-height от 2 до 18')
    own=(a.variant-1)%18+1
    numbers=[own,9 if own==13 else 13]
    rows=[]; counts=[]
    for h in range(2,a.max_height+1,2):
        checks=[]
        for n in numbers:
            root,_,left,right=get_variant(n)
            args=(h,root,left,right)
            # Для обеих обычных реализаций проверяется ограничение условия.
            checks.extend(measure(f,args,1,False) for f in FUNCTIONS[:2])
        if max(checks)>1:
            print('Высота',h,'исключена: обычное построение превысило 1 с; диапазон окончен.')
            break
        for n in numbers:
            root,_,left,right=get_variant(n)
            args=(h,root,left,right)
            for f in FUNCTIONS:
                for warm in (False,True):
                    if warm and not hasattr(f,'cache_clear'): continue
                    seconds=measure(f,args,a.repeat,warm)
                    rows.append([n,h,f.__name__,'warm' if warm else 'cold',seconds])
                clear(f)
                counter={'left':0,'right':0}
                def counted_left(x):
                    counter['left']+=1
                    return left(x)
                def counted_right(x):
                    counter['right']+=1
                    return right(x)
                f(h,root,counted_left,counted_right)
                counts.append([n,h,f.__name__,counter['left'],counter['right']])
                if hasattr(f,'cache_info'): print(n,h,f.cache_info())
                clear(f)
    out=Path(__file__).resolve().parent/'measurements';out.mkdir(exist_ok=True)
    for name,header,data in [('timings.csv',['variant','height','function','scenario','seconds'],rows),
                             ('rule_calls.csv',['variant','height','function','left_calls','right_calls'],counts)]:
        with (out/name).open('w',newline='',encoding='utf-8-sig') as file:
            w=csv.writer(file);w.writerow(header);w.writerows(data)
    print('Сохранено:',out)
    if a.plot:
        try: import matplotlib.pyplot as plt
        except ImportError: raise SystemExit('Для --plot установите matplotlib в своём окружении.')
        for n in numbers:
            for scenario in ('cold','warm'):
                fig,ax=plt.subplots(figsize=(10,6))
                for f in FUNCTIONS:
                    points=[r for r in rows if r[0]==n and r[2]==f.__name__ and r[3]==scenario]
                    if points: ax.plot([r[1] for r in points],[r[4] for r in points],marker='o',label=f.__name__)
                ax.set(xlabel='Height (levels)',ylabel='Mean time, seconds',title=f'Variant {n}: {scenario}')
                ax.legend(fontsize=7);ax.grid(alpha=.3);fig.tight_layout()
                fig.savefig(out/f'variant_{n}_{scenario}.png',dpi=160);plt.close(fig)

if __name__=='__main__': main()
