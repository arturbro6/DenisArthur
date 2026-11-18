import unittest
from hello import greet

class TestGreet(unittest.TestCase):
    def test_name(self):
        self.assertEqual(greet("Артур"), "Привет, Артур")

if __name__ == "__main__":
    unittest.main()
