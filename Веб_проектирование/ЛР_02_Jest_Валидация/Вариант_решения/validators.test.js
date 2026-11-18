const { isValidEmail, isValidPassword, isValidAge, validateUser } = require('./validators');

describe('isValidEmail', () => {
  test('принимает обычный адрес', () => expect(isValidEmail('a@example.com')).toBe(true));
  test('результат корректного адреса truthy', () => expect(isValidEmail('x@y.ru')).toBeTruthy());
  test('нет @', () => expect(isValidEmail('example.com')).toBeFalsy());
  test('нет точки в домене', () => expect(isValidEmail('a@example')).toBe(false));
  test('нет локальной части', () => expect(isValidEmail('@example.com')).toBe(false));
  test('два @', () => expect(isValidEmail('a@@example.com')).toBe(false));
  test('пробел', () => expect(isValidEmail('a b@example.com')).toBe(false));
  test.each([null, undefined, 18, {}, []])('нестроковый аргумент %p', (x) => expect(isValidEmail(x)).toBe(false));
});
describe('isValidPassword', () => {
  test('граница 8 символов', () => expect(isValidPassword('abcdefg1')).toBe(true));
  test('7 символов недостаточно', () => expect(isValidPassword('abcdef1')).toBe(false));
  test('нет цифр', () => expect(isValidPassword('abcdefgh')).toBe(false));
  test('нет букв', () => expect(isValidPassword('12345678')).toBe(false));
  test('кириллические буквы', () => expect(isValidPassword('пароль12')).toBe(true));
  test('пустая строка', () => expect(isValidPassword('')).toBeFalsy());
  test.each([null, undefined, 12345678, {}, []])('нестроковый аргумент %p', (x) => expect(isValidPassword(x)).toBe(false));
});
describe('isValidAge', () => {
  test('нижняя граница', () => expect(isValidAge(14)).toBe(true));
  test('верхняя граница', () => expect(isValidAge(120)).toBe(true));
  test('ниже границы', () => expect(isValidAge(13)).toBe(false));
  test('выше границы', () => expect(isValidAge(121)).toBe(false));
  test('дробный возраст', () => expect(isValidAge(18.5)).toBe(false));
  test('обычное целое', () => expect(isValidAge(18)).toBeTruthy());
  test.each(['18', null, undefined, NaN, Infinity, true, {}])('неверное значение %p', (x) => expect(isValidAge(x)).toBe(false));
});
describe('validateUser (дополнительно)', () => {
  test('все данные корректны', () => {
    expect(validateUser({email:'a@example.com',password:'abcdefg1',age:18})).toEqual({isValid:true,errors:[]});
  });
  test('все три ошибки', () => {
    expect(validateUser({email:'a',password:'123',age:10})).toEqual({isValid:false,errors:['Некорректный email','Некорректный пароль','Некорректный возраст']});
  });
  test('одна ошибка', () => {
    expect(validateUser({email:'a@example.com',password:'abcdefg1',age:13}).errors).toEqual(['Некорректный возраст']);
  });
  test.each([null, undefined, 5])('нет объекта пользователя %p', (x) => expect(validateUser(x).isValid).toBe(false));
});
