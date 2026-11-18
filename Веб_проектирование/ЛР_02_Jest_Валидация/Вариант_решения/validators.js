function isValidEmail(email) {
  return typeof email === 'string' && /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}
function isValidPassword(password) {
  return typeof password === 'string' && password.length >= 8
    && /\p{L}/u.test(password) && /[0-9]/.test(password);
}
function isValidAge(age) {
  return Number.isInteger(age) && age >= 14 && age <= 120;
}
function validateUser(user) {
  const data = user !== null && typeof user === 'object' ? user : {};
  const errors = [];
  if (!isValidEmail(data.email)) errors.push('Некорректный email');
  if (!isValidPassword(data.password)) errors.push('Некорректный пароль');
  if (!isValidAge(data.age)) errors.push('Некорректный возраст');
  return { isValid: errors.length === 0, errors };
}
module.exports = { isValidEmail, isValidPassword, isValidAge, validateUser };
