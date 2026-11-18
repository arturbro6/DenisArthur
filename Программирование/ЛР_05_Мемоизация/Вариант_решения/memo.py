"""Учебные реализации с кэшами. Высота измеряется числом уровней."""
from functools import wraps, lru_cache
from baseline_recursive import gen_bin_tree as gen_bin_tree_recursive
from baseline_iterative import gen_bin_tree as gen_bin_tree_iterative


def memoize(function):
    """Кэшировать вызовы с хешируемыми позиционными/именованными аргументами."""
    cache = {}
    @wraps(function)
    def wrapper(*args, **kwargs):
        key = (args, tuple(sorted(kwargs.items())))
        hash(key)  # Явно проверяем поддержку аргументов ключом словаря.
        if key in cache:
            wrapper.hits += 1
            return cache[key]
        wrapper.misses += 1
        result = function(*args, **kwargs)
        cache[key] = result
        return result
    def cache_clear():
        cache.clear()
        wrapper.hits = 0
        wrapper.misses = 0
    wrapper.cache_clear = cache_clear
    cache_clear()
    return wrapper


def validate_height(height):
    if not isinstance(height, int) or isinstance(height, bool) or height < 0:
        raise ValueError("height должен быть неотрицательным целым числом")


@memoize
def gen_bin_tree_recursive_memo(height=5, root=1,
                                left_leaf=lambda x: x * 2,
                                right_leaf=lambda x: x + 3):
    """Строить рекурсивно, пропуская повторные подзадачи через memoize."""
    validate_height(height)
    if height == 0: return None
    node = {"value": root, "left": None, "right": None}
    if height > 1:
        node["left"] = gen_bin_tree_recursive_memo(height-1, left_leaf(root), left_leaf, right_leaf)
        node["right"] = gen_bin_tree_recursive_memo(height-1, right_leaf(root), left_leaf, right_leaf)
    return node


@lru_cache(maxsize=None)
def gen_bin_tree_recursive_lru(height=5, root=1,
                              left_leaf=lambda x: x * 2,
                              right_leaf=lambda x: x + 3):
    """Та же рекурсия с постоянным lru_cache без ограничения размера."""
    validate_height(height)
    if height == 0: return None
    node = {"value": root, "left": None, "right": None}
    if height > 1:
        node["left"] = gen_bin_tree_recursive_lru(height-1, left_leaf(root), left_leaf, right_leaf)
        node["right"] = gen_bin_tree_recursive_lru(height-1, right_leaf(root), left_leaf, right_leaf)
    return node


def gen_bin_tree_iterative_memo(height=5, root=1,
                                left_leaf=lambda x: x * 2,
                                right_leaf=lambda x: x + 3):
    """Строить готовые поддеревья снизу вверх, без рекурсивных вызовов.

    Кэш существует только в пределах одного вызова.
    """
    validate_height(height)
    if height == 0: return None
    cache = {}
    children = {}
    stack = [(height, root, False)]
    while stack:
        h, value, expanded = stack.pop()
        key = (h, value)
        if key in cache: continue
        if h == 1:
            cache[key] = {"value": value, "left": None, "right": None}
        elif expanded:
            # Значения детей вычислены при первом посещении и сохранены.
            lv, rv = children[key]
            cache[key] = {"value": value,
                          "left": cache[(h-1, lv)], "right": cache[(h-1, rv)]}
        else:
            lv, rv = left_leaf(value), right_leaf(value)
            children[key] = (lv, rv)
            stack.append((h, value, True))
            stack.append((h-1, rv, False))
            stack.append((h-1, lv, False))
    return cache[(height, root)]
