def greet(name: str) -> str:
    """Вернуть приветствие, не печатая его."""
    return "Привет, " + name


if __name__ == "__main__":
    print(greet("Артур"))
