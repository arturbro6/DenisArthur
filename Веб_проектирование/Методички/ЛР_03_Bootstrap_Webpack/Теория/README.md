# Как связаны файлы и инструменты

HTML задаёт структуру страницы. CSS задаёт внешний вид. SCSS — исходный язык с переменными и импортами, который компилируется в CSS. JavaScript отвечает за поведение аккордеона. Webpack проходит от точки входа по импортам и собирает выходные ресурсы.

Цепочка SCSS: sass-loader вызывает Sass → postcss-loader с Autoprefixer обрабатывает CSS → css-loader разбирает зависимости → MiniCssExtractPlugin выделяет итог в styles.css. Массив use читается справа налево. Одновременный style-loader здесь не нужен: он вставлял бы CSS через JS, а требуется отдельный файл.

| Запись | Значение |
|---|---|
| `require('node:path')` | Подключить встроенный модуль путей Node |
| `module.exports = {...}` | Экспорт webpack-конфигурации CommonJS |
| `entry` | Откуда начинается обход импортов |
| `path.resolve(__dirname,'app/dist')` | Абсолютный путь к выходному каталогу |
| `clean:true` | Очистить output.path перед записью |
| `test:/\.scss$/i` | Применить правило к именам, оканчивающимся на .scss |
| `new MiniCssExtractPlugin(...)` | Создать экземпляр плагина |
| `@import` | Подключить SCSS-модуль на этапе компиляции |
| `$accordion-bg` | Переменная SCSS, не CSS-переменная |
| `--bs-...` | CSS-переменная времени выполнения — другой механизм |

Bootstrap SCSS использует функции, переменные, карты и mixins как инфраструктуру. Подключение этой инфраструктуры не означает импорт всех компонентов. Переопределения значений должны стоять после functions, но до variables и генерации CSS. Не редактируйте node_modules: эти изменения исчезнут после npm ci.

У сетки 12 долей. col-12 занимает всю строку; col-md-6 — половину начиная с md; col-lg-4 — треть начиная с lg. Это mobile-first: правило меньшей ширины действует, пока его не переопределит следующее.

Аккордеон использует Collapse: data-bs-target указывает на id раскрываемого блока; data-bs-parent связывает пункты для закрытия предыдущего. Классы show/collapsed и aria-expanded должны соответствовать начальному состоянию. ARIA сообщает состояние вспомогательным технологиям, но само не выполняет анимацию.

Источники: [webpack в Bootstrap](https://getbootstrap.com/docs/5.3/getting-started/webpack/), [Sass](https://getbootstrap.com/docs/5.3/customize/sass/), [аккордеон](https://getbootstrap.com/docs/5.3/components/accordion/), [GitHub Pages custom workflow](https://docs.github.com/en/pages/getting-started-with-github-pages/using-custom-workflows-with-github-pages).
