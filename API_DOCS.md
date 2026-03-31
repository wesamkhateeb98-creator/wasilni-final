# Wasilni API Documentation

**Base URL:** `https://{host}/api/v1.0`

---

## Authentication

جميع الـ endpoints تحتاج `Authorization: Bearer {token}` ما عدا المذكور غير ذلك.

| Role | وصف |
|------|-----|
| `Admin` | مدير النظام |
| `HasBus` | أدمن أو سائق عنده باص مسجّل |

---

## Buses `/buses`

### `POST /buses`
إضافة باص جديد.

**Authorization:** `Admin`

**Request Body:**
```json
{
  "plate": "string",
  "color": "string",
  "lineId": 1,
  "type": 0
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `plate` | string | ✅ | رقم اللوحة |
| `color` | string | ✅ | |
| `lineId` | int | ❌ | nullable |
| `type` | BusType | ✅ | enum |

**Response:**
```json
{ "id": 1 }
```

---

### `GET /buses`
جلب قائمة الباصات للأدمن.

**Authorization:** `Admin`

**Query Params:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| `pageNumber` | int | ✅ | |
| `pageSize` | int | ✅ | |
| `ownerId` | int | ❌ | فلتر بالمالك |
| `plate` | string | ❌ | فلتر باللوحة |
| `filter` | BusTypeFilter | ✅ | `0` = All |

**Response:**
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5,
  "content": [
    {
      "busId": 1,
      "plate": "ABC-123",
      "color": "Red",
      "type": 0,
      "numberOfSeats": 0,
      "lineId": 3,
      "driver": { "id": 12, "name": "Ahmed" }
    }
  ]
}
```

---

### `PUT /buses/{id}`
تعديل بيانات باص.

**Authorization:** `Admin`

**Request Body:**
```json
{
  "plate": "string",
  "color": "string",
  "lineId": 1,
  "type": 0
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `plate` | string | ✅ | |
| `color` | string | ✅ | |
| `lineId` | int | ❌ | nullable |
| `type` | BusType | ✅ | |

**Response:**
```json
{ "id": 1 }
```

---

### `DELETE /buses/{id}`
حذف باص.

**Authorization:** `Admin`

**Response:**
```json
{ "id": 1 }
```

---

### `PATCH /buses/{id}/driver`
تعيين سائق على باص.

**Authorization:** `Admin`

**Request Body:**
```json
{ "driverId": 12 }
```

**Response:**
```json
{ "id": 1 }
```

---

### `DELETE /buses/{id}/driver`
إزالة السائق من الباص.

**Authorization:** `Admin`

**Response:**
```json
{ "id": 1 }
```

---

## Lines `/lines`

### `GET /lines`
جلب قائمة الخطوط.

**Authorization:** `HasBus` (أدمن أو سائق عنده باص)

**Query Params:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| `pageNumber` | int | ✅ | |
| `pageSize` | int | ✅ | |
| `name` | string | ❌ | فلتر بالاسم |

**Response:**
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 2,
  "content": [
    { "id": 1, "name": "Main Line" }
  ]
}
```

---

## Users `/accounts`

### `GET /accounts`
جلب قائمة المستخدمين.

**Authorization:** `Admin`

**Query Params:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| `pageNumber` | int | ✅ | |
| `pageSize` | int | ✅ | |
| `phoneNumber` | string | ❌ | بحث بالهاتف |

**Response:**
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 3,
  "content": [
    { "id": 1, "name": "Ahmed", "phoneNumber": "0911000001" }
  ]
}
```

---

## Reports `/reports`

### `GET /reports`
جلب تقارير الركاب (يومية / شهرية / سنوية).

**Authorization:** `Admin`

**Query Params:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| `type` | ReportType | ✅ | `0`=Daily, `1`=Monthly, `2`=Yearly |
| `from` | DateTime | ✅ | تاريخ البداية |
| `to` | DateTime | ✅ | تاريخ النهاية |
| `lineId` | int | ❌ | فلتر بالخط |

**Validation:**
- `from` يجب أن يكون ≤ `to`

**Response حسب النوع:**

**Daily** — صف لكل يوم:
```json
[
  { "busId": null, "lineId": 3, "year": 2026, "month": 3, "day": "2026-03-01", "totalRiders": 42 }
]
```

**Monthly** — صف لكل شهر:
```json
[
  { "busId": null, "lineId": 3, "year": 2026, "month": 3, "day": null, "totalRiders": 1200 }
]
```

**Yearly** — صف لكل سنة:
```json
[
  { "busId": null, "lineId": null, "year": 2026, "month": null, "day": null, "totalRiders": 14500 }
]
```

---

## Enums

### `BusType`
| Value | Name |
|-------|------|
| `0` | Bolman |
| `1` | Van |
| `2` | Servece |

### `ReportType`
| Value | Name |
|-------|------|
| `0` | Daily |
| `1` | Monthly |
| `2` | Yearly |

### `BusTypeFilter`
| Value | Name |
|-------|------|
| `0` | All |
| `1` | Bolman |
| `2` | Van |
| `3` | Servece |
