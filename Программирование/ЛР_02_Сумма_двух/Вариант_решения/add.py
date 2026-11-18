def add(nums: list[int], target: int) -> list[int] | None:
    """Найти два разных индекса простым перебором, иначе вернуть None."""
    for i in range(len(nums)):
        for j in range(i + 1, len(nums)):
            if nums[i] + nums[j] == target:
                return [i, j]
    return None


def add_fast(nums: list[int], target: int) -> list[int] | None:
    """Найти пару, сохраняя индексы уже просмотренных чисел."""
    seen = {}
    for i, value in enumerate(nums):
        missing = target - value
        if missing in seen:
            return [seen[missing], i]
        seen[value] = i
    return None
