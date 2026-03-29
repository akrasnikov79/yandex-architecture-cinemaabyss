План: Обновить Content Service — реалистичная архитектура видеостриминга
Контекст
Content Service на диаграмме показывает стриминг видео через HTTP/REST и API Gateway — это нереалистично. Видеопоток в онлайн-кинотеатрах доставляется через CDN и специализированные протоколы (HLS/DASH), минуя API Gateway. Нужно разделить Content Service на API-часть и медиа-доставку.

Изменения в docs/Container.puml
1. Переименовать Content Service → Content API
Управляет каталогом контента, правами доступа, генерирует подписанные URL для плейлистов. Не стримит видео.

Container(content_svc, "Content Service", "Go", "Управление видеоконтентом,\nправа доступа, генерация\nподписанных URL для плейлистов")
2. Добавить CDN / Media Server
Внешняя система (или внутренний контейнер), которая доставляет видеосегменты клиенту по HLS/DASH.

System_Ext(cdn, "CDN / Media Server", "Nginx VOD / CloudFront.\nДоставка видео по HLS/DASH.\nКэширование сегментов")
3. Добавить Object Storage
Хранение видеофайлов и HLS/DASH-сегментов (не PostgreSQL).

ContainerDb(storage, "Object Storage", "S3 / MinIO", "Видеофайлы,\nHLS/DASH-сегменты")
4. Обновить связи
Убрать:

proxy → content_svc с пометкой "streaming" (стриминг не идёт через gateway)
Добавить:

user → cdn — клиент получает видеопоток напрямую из CDN (HLS/DASH)
proxy → content_svc — только API: каталог, права, URL плейлистов
content_svc → storage — управление видеофайлами
cdn → storage — CDN читает сегменты из хранилища
5. Обновить db_content
Переименовать — БД хранит только метаданные просмотров (таблица views), а не видеофайлы.

ContainerDb(db_content, "БД просмотров", "PostgreSQL", "История просмотров,\nпрогресс, статистика")
Итоговая схема доставки видео
Клиент ──REST──→ API Gateway → Content Service → выдаёт подписанный URL плейлиста
Клиент ──HLS/DASH──→ CDN / Media Server → Object Storage (видеосегменты)
Content Service → Kafka (события: начало/конец просмотра)
Content Service → db_content (прогресс просмотра)
Content Service → storage (управление файлами)
Файл
Файл	Действие
docs/Container.puml	Изменить