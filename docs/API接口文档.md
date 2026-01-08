# UIPS API 接口文档

> 基础地址：`https://localhost:7149`  
> 认证方式：JWT Bearer Token  
> 内容类型：`application/json`（除文件上传外）

---

## 一、认证模块 (AuthController)

### 1.1 用户注册

创建新用户账号。

```
POST /api/auth/users
```

**请求体：**
```json
{
  "userName": "string",
  "password": "string"
}
```

**响应：**
| 状态码 | 说明 |
|--------|------|
| 201 | 注册成功 |
| 400 | 用户名已存在 |

**示例响应：**
```json
"注册成功"
```

---

### 1.2 用户登录

获取 JWT 访问令牌。

```
POST /api/auth/token
```

**请求体：**
```json
{
  "userName": "string",
  "password": "string"
}
```

**响应：**
| 状态码 | 说明 |
|--------|------|
| 200 | 登录成功，返回 Token |
| 401 | 用户名或密码错误 |

**成功响应：**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "a1b2c3d4e5f6...",
  "userId": 1,
  "userName": "testuser",
  "expiresIn": 7200,
  "role": "User"
}
```

---

## 二、图片模块 (ImageController)

> 🔒 除预览接口外，所有接口需要 JWT 认证

### 2.1 上传单张图片

```
POST /api/images
```

**请求头：**
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**请求体：**
| 字段 | 类型 | 说明 |
|------|------|------|
| file | File | 图片文件 |

**响应：**
| 状态码 | 说明 |
|--------|------|
| 201 | 上传成功 |
| 400 | 文件为空 |
| 401 | 未认证 |

**成功响应：**
```json
{
  "id": 1,
  "originalFileName": "photo.jpg",
  "uploadedAt": "2026-01-09T10:30:00Z",
  "fileSize": 102400,
  "url": "/api/images/1/preview"
}
```

---

### 2.2 批量上传图片

```
POST /api/images/batch
```

**请求头：**
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**请求体：**
| 字段 | 类型 | 说明 |
|------|------|------|
| files | File[] | 图片文件数组 |

**响应：**
| 状态码 | 说明 |
|--------|------|
| 201 | 上传成功 |
| 400 | 文件列表为空 |
| 401 | 未认证 |

---

### 2.3 获取图片列表（分页）

```
GET /api/images?PageIndex={pageIndex}&PageSize={pageSize}
```

**请求头：**
```
Authorization: Bearer {token}
```

**查询参数：**
| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| PageIndex | int | 1 | 页码 |
| PageSize | int | 10 | 每页数量 |

**成功响应：**
```json
{
  "items": [
    {
      "id": 1,
      "originalFileName": "photo.jpg",
      "uploadedAt": "2026-01-09T10:30:00Z",
      "fileSize": 102400,
      "previewUrl": "/api/images/1/file",
      "isSelected": false
    }
  ],
  "totalCount": 50,
  "pageIndex": 1,
  "pageSize": 10
}
```

---

### 2.4 获取唯一文件名列表

```
GET /api/images/filenames
```

**请求头：**
```
Authorization: Bearer {token}
```

**成功响应：**
```json
["photo1.jpg", "photo2.png", "image.gif"]
```

---

### 2.5 根据文件名获取图片

```
GET /api/images/by-filename/{fileName}
```

**路径参数：**
| 参数 | 类型 | 说明 |
|------|------|------|
| fileName | string | 原始文件名 |

---

### 2.6 获取图片预览（公开）

```
GET /api/images/{id}/preview
```

**路径参数：**
| 参数 | 类型 | 说明 |
|------|------|------|
| id | int | 图片 ID |

**响应：** 图片文件流（无需认证）

---

### 2.7 获取图片文件（需认证）

```
GET /api/images/{id}/file
```

**请求头：**
```
Authorization: Bearer {token}
```

**响应：**
| 状态码 | 说明 |
|--------|------|
| 200 | 返回图片文件流 |
| 401 | 未认证 |
| 403 | 无权访问（非本人图片） |
| 404 | 图片不存在 |

---

### 2.8 删除图片

```
DELETE /api/images/{id}
```

**请求头：**
```
Authorization: Bearer {token}
```

**响应：**
| 状态码 | 说明 |
|--------|------|
| 204 | 删除成功 |
| 401 | 未认证 |
| 403 | 无权删除 |
| 404 | 图片不存在 |

---

### 2.9 切换收藏状态

```
PUT /api/images/{id}/favourite
```

**请求头：**
```
Authorization: Bearer {token}
```

**成功响应：**
```json
{
  "isSelected": true
}
```

---

## 三、管理员模块 (AdminController)

> 🔒 所有接口需要 Admin 角色

### 3.1 获取用户列表（分页）

```
GET /api/admin/users?pageIndex={pageIndex}&pageSize={pageSize}
```

**成功响应：**
```json
{
  "items": [
    {
      "id": 1,
      "userName": "admin",
      "role": "Admin"
    },
    {
      "id": 2,
      "userName": "testuser",
      "role": "User"
    }
  ],
  "totalCount": 10,
  "pageIndex": 1,
  "pageSize": 10
}
```

---

### 3.2 获取统计信息

```
GET /api/admin/statistics
```

**成功响应：**
```json
{
  "totalUsers": 100,
  "totalAdmins": 2,
  "totalImages": 500,
  "totalFavourites": 150
}
```

---

### 3.3 更新用户角色

```
PUT /api/admin/users/{userId}/role
```

**路径参数：**
| 参数 | 类型 | 说明 |
|------|------|------|
| userId | int | 用户 ID |

**请求体：**
```json
{
  "role": "Admin"
}
```

> role 只能是 `"User"` 或 `"Admin"`

**响应：**
| 状态码 | 说明 |
|--------|------|
| 200 | 更新成功 |
| 400 | 角色值无效 / 不能修改自己 |
| 404 | 用户不存在 |

---

### 3.4 删除用户

```
DELETE /api/admin/users/{userId}
```

**响应：**
| 状态码 | 说明 |
|--------|------|
| 204 | 删除成功（同时删除用户的图片和收藏） |
| 400 | 不能删除自己 |
| 404 | 用户不存在 |

---

### 3.5 重置用户密码

```
PUT /api/admin/users/{userId}/password
```

**请求体：**
```json
{
  "newPassword": "newpassword123"
}
```

**响应：**
| 状态码 | 说明 |
|--------|------|
| 200 | 重置成功 |
| 404 | 用户不存在 |

---

### 3.6 获取所有图片（管理员视图）

```
GET /api/admin/images?pageIndex={pageIndex}&pageSize={pageSize}
```

**成功响应：**
```json
{
  "items": [
    {
      "id": 1,
      "originalFileName": "photo.jpg",
      "uploadedAt": "2026-01-09T10:30:00Z",
      "fileSize": 102400,
      "ownerName": "testuser",
      "ownerId": 2,
      "previewUrl": "/api/images/1/file"
    }
  ],
  "totalCount": 500,
  "pageIndex": 1,
  "pageSize": 10
}
```

---

### 3.7 批量删除图片

```
DELETE /api/admin/images/batch
```

**请求体：**
```json
{
  "imageIds": [1, 2, 3, 4, 5]
}
```

**成功响应：**
```json
{
  "message": "成功删除 5 张图片"
}
```

---

## 四、通用数据结构

### 分页请求 (PaginatedRequestDto)
```json
{
  "pageIndex": 1,
  "pageSize": 10
}
```

### 分页响应 (PaginatedResult<T>)
```json
{
  "items": [],
  "totalCount": 0,
  "pageIndex": 1,
  "pageSize": 10
}
```

---

## 五、错误响应格式

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "错误详情信息"
}
```

---

## 六、认证说明

1. 调用登录接口获取 `accessToken`
2. 在后续请求的 Header 中添加：
   ```
   Authorization: Bearer {accessToken}
   ```
3. Token 有效期为 120 分钟（7200 秒）
4. 管理员接口需要 `role: "Admin"` 的用户

---

## 七、接口速查表

| 方法 | 路径 | 说明 | 认证 |
|------|------|------|------|
| POST | `/api/auth/users` | 用户注册 | ❌ |
| POST | `/api/auth/token` | 用户登录 | ❌ |
| POST | `/api/images` | 上传图片 | ✅ |
| POST | `/api/images/batch` | 批量上传 | ✅ |
| GET | `/api/images` | 获取图片列表 | ✅ |
| GET | `/api/images/filenames` | 获取文件名列表 | ✅ |
| GET | `/api/images/by-filename/{name}` | 按文件名查询 | ✅ |
| GET | `/api/images/{id}/preview` | 图片预览 | ❌ |
| GET | `/api/images/{id}/file` | 获取图片文件 | ✅ |
| DELETE | `/api/images/{id}` | 删除图片 | ✅ |
| PUT | `/api/images/{id}/favourite` | 切换收藏 | ✅ |
| GET | `/api/admin/users` | 用户列表 | 🔐 Admin |
| GET | `/api/admin/statistics` | 统计信息 | 🔐 Admin |
| PUT | `/api/admin/users/{id}/role` | 更新角色 | 🔐 Admin |
| DELETE | `/api/admin/users/{id}` | 删除用户 | 🔐 Admin |
| PUT | `/api/admin/users/{id}/password` | 重置密码 | 🔐 Admin |
| GET | `/api/admin/images` | 所有图片 | 🔐 Admin |
| DELETE | `/api/admin/images/batch` | 批量删除图片 | 🔐 Admin |
