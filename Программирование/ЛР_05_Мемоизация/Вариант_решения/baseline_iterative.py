from collections import deque
from typing import Callable, Any

Number = int | float
Tree = dict[str, Any] | None


def gen_bin_tree(height: int = 5, root: Number = 1,
                 left_leaf: Callable[[Number], Number] = lambda x: x * 2,
                 right_leaf: Callable[[Number], Number] = lambda x: x + 3) -> Tree:
    """Построить дерево по уровням без рекурсии; height — число уровней."""
    if not isinstance(height, int) or isinstance(height, bool) or height < 0:
        raise ValueError("height должен быть неотрицательным целым числом")
    if height == 0:
        return None
    tree = {"value": root, "left": None, "right": None}
    queue = deque([(tree, 1)])
    while queue:
        node, level = queue.popleft()
        if level == height:
            continue
        left_value = left_leaf(node["value"])
        right_value = right_leaf(node["value"])
        node["left"] = {"value": left_value, "left": None, "right": None}
        node["right"] = {"value": right_value, "left": None, "right": None}
        queue.append((node["left"], level + 1))
        queue.append((node["right"], level + 1))
    return tree
