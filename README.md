# AccountingWebsite 記帳網站

這是一個使用 ASP.NET Core 6 MVC 開發的個人記帳網站，提供了簡單的功能來管理個人財務。

## 主要功能

1. 用戶管理
   - 註冊新用戶
   - 用戶登入和登出

2. 交易記錄
   - 新增收入和支出記錄
   - 查看特定日期的交易記錄
   - 編輯和刪除交易記錄
   - 按日期排序交易記錄

3. 餘額管理
   - 自動計算和更新用戶餘額
   - 手動調整餘額功能

4. 月度報告
   - 查看特定月份的收支摘要
   - 計算月度總收入和總支出
   - 顯示月度淨收入

5. 安全性
   - 使用 JWT（JSON Web Token）進行身份驗證
   - 密碼加密存儲
   - 防止跨站請求偽造（CSRF）攻擊

## 技術棧

- ASP.NET Core 6 MVC
- Entity Framework Core
- Microsoft Identity
- JWT 認證
- HTML/CSS
