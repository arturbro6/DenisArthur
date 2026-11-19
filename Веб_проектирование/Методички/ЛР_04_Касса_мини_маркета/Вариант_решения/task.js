function task(order) {
  const catalog = {
    notebook: { price: 250, stock: 8 },
    marker: { price: 90, stock: 12 },
    folder: { price: 120, stock: 6 }
  };
  const owns = (object, key) => Object.prototype.hasOwnProperty.call(object, key);
  let subtotal = 0;
  let quantity = 0;

  for (const code in order.items) {
    if (!owns(order.items, code)) continue;
    if (!owns(catalog, code)) {
      return { ok: false, error: "unknown-item" };
    }
    const count = order.items[code];
    if (!Number.isInteger(count) || count < 0) {
      return { ok: false, error: "invalid-quantity" };
    }
    if (count > catalog[code].stock) {
      return { ok: false, error: "out-of-stock" };
    }
    quantity += count;
    subtotal += catalog[code].price * count;
  }
  if (quantity === 0) {
    return { ok: false, error: "empty-cart" };
  }
  if (order.promo !== "" && order.promo !== "STUDENT") {
    return { ok: false, error: "bad-promo" };
  }

  let discount = 0;
  if (order.promo === "STUDENT") {
    discount = Math.floor(subtotal / 10);
  }
  const afterDiscount = subtotal - discount;
  let delivery = 150;
  if (order.pickup || afterDiscount >= 1000) {
    delivery = 0;
  }
  const total = afterDiscount + delivery;
  return { ok: true, subtotal, discount, delivery, total };
}
