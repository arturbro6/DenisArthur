const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const context = {};
vm.createContext(context);
vm.runInContext(fs.readFileSync(path.join(__dirname, 'task.js'), 'utf8'), context);
const task = order => JSON.parse(JSON.stringify(context.task(order)));
let checked = 0;
function check(order, expected) {
  const before = JSON.stringify(order);
  assert.deepEqual(task(order), expected);
  assert.equal(JSON.stringify(order), before);
  assert.deepEqual(task(order), expected);
  checked++;
}
function ok(subtotal, discount, delivery) {
  return {ok:true, subtotal, discount, delivery, total:subtotal-discount+delivery};
}
const order = (items, promo='', pickup=false) => ({items,promo,pickup});
check(order({notebook:2,marker:1},'STUDENT'),ok(590,59,150));
check(order({notebook:4}),ok(1000,0,0));
check(order({notebook:4},'STUDENT'),ok(1000,100,150));
check(order({folder:1},'',true),ok(120,0,0));
check(order({notebook:9}),{ok:false,error:'out-of-stock'});
for (const bad of [-1,0.5,'2',true,null,undefined,NaN,Infinity,{},[]]) {
  check(order({marker:bad}),{ok:false,error:'invalid-quantity'});
}
for (const key of ['coffee','toString','constructor','hasOwnProperty','__proto__']) {
  const items=Object.create(null);items[key]=1;
  check(order(items),{ok:false,error:'unknown-item'});
}
check(order({}),{ok:false,error:'empty-cart'});
check(order({notebook:0,marker:0,folder:0}),{ok:false,error:'empty-cart'});
for(const promo of ['SAVE','student',' STUDENT','STUDENT ']) {
  check(order({folder:1},promo),{ok:false,error:'bad-promo'});
}
check(order({marker:13}),{ok:false,error:'out-of-stock'});
check(order({folder:7}),{ok:false,error:'out-of-stock'});
const frozen=Object.freeze({items:Object.freeze({notebook:2}),promo:'STUDENT',pickup:false});
check(frozen,ok(500,50,150));
// Каждый допустимый набор количеств, оба промокода и способа получения.
for(let n=0;n<=8;n++) for(let m=0;m<=12;m++) for(let f=0;f<=6;f++) {
  for(const promo of ['', 'STUDENT']) for(const pickup of [false,true]) {
    const input=order({notebook:n,marker:m,folder:f},promo,pickup);
    if(n+m+f===0){check(input,{ok:false,error:'empty-cart'});continue;}
    const subtotal=n*250+m*90+f*120;
    const discount=promo==='STUDENT'?Math.floor(subtotal*10/100):0;
    const delivery=pickup?0:(subtotal-discount>=1000?0:150);
    check(input,ok(subtotal,discount,delivery));
  }
}
console.log(`OK: ${checked} сценариев, каждый с повторным вызовом и проверкой неизменности входа.`);
