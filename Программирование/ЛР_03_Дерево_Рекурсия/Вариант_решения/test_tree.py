import unittest
from tree import gen_bin_tree
from variants import get_variant

class TestTree(unittest.TestCase):
    def test_empty(self): self.assertIsNone(gen_bin_tree(height=0))
    def test_root(self):
        self.assertEqual(gen_bin_tree(height=1, root=0), {"value": 0, "left": None, "right": None})
    def test_two_levels(self):
        self.assertEqual(gen_bin_tree(height=2, root=3), {
            "value": 3, "left": {"value": 6, "left": None, "right": None},
            "right": {"value": 6, "left": None, "right": None}})
    def test_rules_apply_to_current_node(self):
        tree = gen_bin_tree(3, 1, lambda x: x + 1, lambda x: x - 1)
        self.assertEqual(tree["left"]["left"]["value"], 3)
        self.assertEqual(tree["left"]["right"]["value"], 1)
    def test_different_objects(self):
        tree = gen_bin_tree(3, 1, lambda x: x + 1, lambda x: x - 1)
        self.assertEqual(tree["left"]["right"], tree["right"]["left"])
        self.assertIsNot(tree["left"]["right"], tree["right"]["left"])
    def test_invalid_height(self):
        for value in (-1, 1.5, True):
            with self.subTest(value=value), self.assertRaises(ValueError):
                gen_bin_tree(value)
    def test_all_variants(self):
        for number in range(1, 19):
            root, height, left, right = get_variant(number)
            with self.subTest(variant=number):
                tree = gen_bin_tree(height, root, left, right)
                self.assertEqual(tree["value"], root)
                self.assertEqual(tree["left"]["value"], left(root))
                self.assertEqual(tree["right"]["value"], right(root))
    def test_variant_wrap(self): self.assertIs(get_variant(1), get_variant(19))

if __name__ == "__main__": unittest.main()
