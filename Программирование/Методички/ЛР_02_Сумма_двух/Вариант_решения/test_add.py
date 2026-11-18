import unittest
from add import add, add_fast

class TestAdd(unittest.TestCase):
    def check_pair(self, nums, target):
        for function in (add, add_fast):
            with self.subTest(function=function.__name__):
                result = function(nums, target)
                self.assertIsNotNone(result)
                self.assertEqual(len(result), 2)
                i, j = result
                self.assertIsInstance(i, int)
                self.assertIsInstance(j, int)
                self.assertNotEqual(i, j)
                self.assertTrue(0 <= i < len(nums))
                self.assertTrue(0 <= j < len(nums))
                self.assertEqual(nums[i] + nums[j], target)

    def test_basic(self): self.check_pair([2, 7, 11, 15], 9)
    def test_pair_at_end(self): self.check_pair([3, 2, 4], 6)
    def test_duplicates(self): self.check_pair([3, 3], 6)
    def test_negatives(self): self.check_pair([-8, -3, 7], -11)
    def test_zero(self): self.check_pair([0, 5], 5)
    def test_two_zeroes(self): self.check_pair([0, 0], 0)
    def test_multiple_solutions(self): self.check_pair([1, 2, 3, 4], 5)
    def test_no_pair(self):
        for function in (add, add_fast): self.assertIsNone(function([1, 2, 3], 10))
    def test_empty(self):
        for function in (add, add_fast): self.assertIsNone(function([], 0))
    def test_single_element(self):
        for function in (add, add_fast): self.assertIsNone(function([3], 6))
    def test_does_not_modify_input(self):
        nums = [7, 2, 5]
        before = nums.copy()
        self.check_pair(nums, 9)
        self.assertEqual(nums, before)

if __name__ == "__main__": unittest.main()
