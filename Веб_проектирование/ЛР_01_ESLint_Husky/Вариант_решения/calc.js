export function add(a, b) { return a + b; }
export function sub(a, b) { return a - b; }
export function mul(a, b) { return a * b; }
export function div(a, b) {
  if (b === 0) throw new Error('Деление на ноль');
  return a / b;
}
console.log(add(2, 3), sub(9, 4), mul(2, 4), div(8, 2));
