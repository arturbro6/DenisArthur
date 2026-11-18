from typing import Callable, Any

Number = int | float
Tree = dict[str, Any] | None


def gen_bin_tree(height: int = 5, root: Number = 1,
                 left_leaf: Callable[[Number], Number] = lambda x: x * 2,
                 right_leaf: Callable[[Number], Number] = lambda x: x + 3) -> Tree:
    """Построить полное бинарное дерево; height — число уровней.

    При height=0 результат None. Правила потомков применяются к текущему узлу.
    """
    if not isinstance(height, int) or isinstance(height, bool) or height < 0:
        raise ValueError("height должен быть неотрицательным целым числом")
    if height == 0:
        return None
    node = {"value": root, "left": None, "right": None}
    if height > 1:
        node["left"] = gen_bin_tree(height - 1, left_leaf(root), left_leaf, right_leaf)
        node["right"] = gen_bin_tree(height - 1, right_leaf(root), left_leaf, right_leaf)
    return node
