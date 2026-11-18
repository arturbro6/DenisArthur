import unittest
from memo import (memoize, gen_bin_tree_recursive as plain,
                  gen_bin_tree_iterative as iterative,
                  gen_bin_tree_recursive_memo as cached,
                  gen_bin_tree_iterative_memo as cached_iterative,
                  gen_bin_tree_recursive_lru as lru)

class TestMemo(unittest.TestCase):
    def setUp(self): cached.cache_clear(); lru.cache_clear()
    def tearDown(self): cached.cache_clear(); lru.cache_clear()
    def test_defaults_equal(self):
        expected = plain()
        for f in (iterative, cached, cached_iterative, lru): self.assertEqual(f(), expected)
    def test_custom_parameters(self):
        for h in (0, 1, 3, 5):
            args = (h, 3, lambda x: x+1, lambda x: x-1)
            for f in (iterative, cached, cached_iterative, lru):
                self.assertEqual(f(*args), plain(*args))
    def test_decorator_counts_and_kwargs(self):
        calls = []
        @memoize
        def double(x):
            """Удвоение."""
            calls.append(x)
            return x*2
        self.assertEqual(double(x=4), 8)
        self.assertEqual(double(x=4), 8)
        self.assertEqual(double(x=5), 10)
        self.assertEqual(calls, [4, 5])
        self.assertEqual((double.hits, double.misses), (1, 2))
        self.assertEqual(double.__name__, "double")
        self.assertEqual(double.__doc__, "Удвоение.")
        double.cache_clear()
        self.assertEqual((double.hits, double.misses), (0, 0))
        double(x=4)
        self.assertEqual(calls, [4, 5, 4])
    def test_kwargs_order(self):
        @memoize
        def difference(a, b): return a-b
        self.assertEqual(difference(a=5,b=2), 3)
        self.assertEqual(difference(b=2,a=5), 3)
        self.assertEqual(difference.hits, 1)
    def test_shared_mutation(self):
        left, right = lambda x:x+1, lambda x:x-1
        tree = cached(3, 1, left, right)
        shared = tree["left"]["right"]
        self.assertIs(shared, tree["right"]["left"])
        shared["value"] = 999
        self.assertEqual(tree["right"]["left"]["value"], 999)
        self.assertIs(cached(3,1,left,right), tree)
        self.assertEqual(cached(3,1,left,right)["left"]["right"]["value"], 999)
        ordinary = plain(3,1,left,right)
        self.assertIsNot(ordinary["left"]["right"], ordinary["right"]["left"])
    def test_iterative_sharing_local_cache(self):
        left,right = lambda x:x+1,lambda x:x-1
        first=cached_iterative(3,1,left,right)
        self.assertIs(first["left"]["right"],first["right"]["left"])
        self.assertIsNot(first,cached_iterative(3,1,left,right))
    def test_recursive_calls_hit_cache(self):
        cached(5,1,lambda x:x+1,lambda x:x-1)
        self.assertGreater(cached.hits,0)
    def test_lru_sharing(self):
        tree=lru(3,1,lambda x:x+1,lambda x:x-1)
        self.assertIs(tree["left"]["right"],tree["right"]["left"])
        self.assertGreater(lru.cache_info().hits,0)

if __name__ == "__main__": unittest.main()
