"""Все 18 вариантов из условия. ^ в математической записи трактуется как степень.
В Python степень записывается **; / сохраняется как обычное деление.
"""
VARIANTS = {
    1: (1, 5, lambda x: x * 2, lambda x: x + 3),
    2: (2, 6, lambda x: x * 3, lambda x: x + 4),
    3: (3, 4, lambda x: x + 2, lambda x: x * 3),
    4: (4, 4, lambda x: x * 4, lambda x: x + 1),
    5: (5, 6, lambda x: x ** 2, lambda x: x - 2),
    6: (6, 5, lambda x: x * 2 - 2, lambda x: x + 4),
    7: (7, 4, lambda x: x * 3, lambda x: x - 4),
    8: (8, 4, lambda x: x + x / 2, lambda x: x ** 2),
    9: (9, 6, lambda x: x * 2 + 1, lambda x: 2 * x - 1),
    10: (10, 5, lambda x: x * 3 + 1, lambda x: 3 * x - 1),
    11: (11, 3, lambda x: x ** 2, lambda x: 2 + x ** 2),
    12: (12, 4, lambda x: x ** 3, lambda x: x * 2 - 1),
    13: (13, 3, lambda x: x + 1, lambda x: x - 1),
    14: (14, 4, lambda x: 2 - (x - 1), lambda x: x * 2),
    15: (15, 6, lambda x: 2 * (x + 1), lambda x: 2 * (x - 1)),
    16: (16, 3, lambda x: x / 2, lambda x: x * 2),
    17: (17, 4, lambda x: (x - 4) ** 2, lambda x: (x + 3) * 2),
    18: (18, 5, lambda x: (x - 8) * 3, lambda x: (x + 8) * 2),
}

def get_variant(number):
    """Номер студента переводится в вариант циклически: 19 -> 1."""
    if not isinstance(number, int) or isinstance(number, bool) or number < 1:
        raise ValueError("Номер должен быть положительным целым числом")
    return VARIANTS[(number - 1) % 18 + 1]
