using System.Linq;
using System.Web.Mvc;
using Warehouse.Models; 

namespace Warehouse.Controllers
{
    public class HomeController : Controller
    {
        private Warehouse_22DH113899Entities db = new Warehouse_22DH113899Entities();

        public ActionResult Index()
        {
            var products = db.Products.ToList();
            return View(products);
        }
        // 1. GET: Mở form sửa
        public ActionResult Edit(int id)
        {
            // Tìm sản phẩm theo ID
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            return View(product);
        }

        // 2. POST: Nhận dữ liệu sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Dùng Bind Include để ép Controller chỉ nhận Tên, Danh mục, Giá (bỏ qua Quantity từ View gửi lên nếu có)
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,Category,Price")] Product product)
        {
            // BƯỚC 1: Kiểm tra điều kiện (Validation) theo yêu cầu đề bài
            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                ModelState.AddModelError("ProductName", "Tên sản phẩm không được để trống!");
            }
            if (string.IsNullOrWhiteSpace(product.Category))
            {
                ModelState.AddModelError("Category", "Danh mục không được để trống!");
            }
            if (product.Price <= 0)
            {
                ModelState.AddModelError("Price", "Đơn giá phải lớn hơn 0!");
            }

            // BƯỚC 2: Nếu dữ liệu hợp lệ thì lưu CSDL
            if (ModelState.IsValid)
            {
                // Lấy record cũ từ CSDL lên
                var existingProduct = db.Products.Find(product.ProductID);
                if (existingProduct != null)
                {
                    // Chỉ cập nhật 3 cột được phép
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.Category = product.Category;
                    existingProduct.Price = product.Price;

                    // TUYỆT ĐỐI KHÔNG gán existingProduct.Quantity = product.Quantity;

                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            // Nếu có lỗi, trả về lại form kèm thông báo đỏ
            return View(product);
        }
        // GET: Home/Search
        public ActionResult Search(string keyword, int? status, string sortOrder)
        {
            // 1. Lấy toàn bộ dữ liệu ra dưới dạng IQueryable để chuẩn bị lọc
            var products = db.Products.AsQueryable();

            // 2. Lọc theo tên (Không phân biệt hoa thường)[cite: 9]
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                products = products.Where(p => p.ProductName.ToLower().Contains(keyword));
            }

            // 3. Lọc theo tình trạng (0: Hết hàng, 1: Sắp hết, 2: Đủ hàng)[cite: 9]
            if (status.HasValue)
            {
                if (status == 0)
                    products = products.Where(p => p.Quantity == 0);
                else if (status == 1)
                    products = products.Where(p => p.Quantity >= 1 && p.Quantity <= 4);
                else if (status == 2)
                    products = products.Where(p => p.Quantity >= 5);
            }

            // 4. Sắp xếp theo giá (Tăng dần / Giảm dần)[cite: 9]
            if (sortOrder == "asc")
            {
                products = products.OrderBy(p => p.Price);
            }
            else if (sortOrder == "desc")
            {
                products = products.OrderByDescending(p => p.Price);
            }

            // 5. Xử lý thông báo nếu không có kết quả[cite: 9]
            if (!products.Any())
            {
                ViewBag.Message = "Không tìm thấy sản phẩm nào phù hợp với điều kiện tìm kiếm!";
            }

            // Giữ lại giá trị trên form (tùy chọn để giao diện thân thiện hơn)
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.SortOrder = sortOrder;

            return View(products.ToList());
        }
        // 1. Mở trang Kiểm kê sản phẩm (GET)
        public ActionResult KiemKe()
        {
            var products = db.Products.ToList();
            return View(products);
        }

        // 2. Xử lý cập nhật số lượng (POST)
        [HttpPost]
        public ActionResult CapNhatKiemKe(int id, int? actualQuantity)
        {
            // BẮT LỖI (Test 4.4): Nếu người dùng bỏ trống, nhập chữ, hoặc nhập số âm
            if (actualQuantity == null || actualQuantity < 0)
            {
                TempData["Error"] = "Số lượng thực tế không hợp lệ! Phải là số nguyên không âm.";
                return RedirectToAction("KiemKe");
            }

            var product = db.Products.Find(id);
            if (product != null)
            {
                int oldQty = product.Quantity;
                int newQty = actualQuantity.Value;
                int diff = System.Math.Abs(newQty - oldQty); // Lấy trị tuyệt đối độ chênh lệch

                // XỬ LÝ LOGIC CHÊNH LỆCH
                if (newQty > oldQty)
                {
                    TempData["Success"] = $"Chênh lệch tăng {diff} (Cập nhật '{product.ProductName}' từ {oldQty} lên {newQty}).";
                }
                else if (newQty < oldQty)
                {
                    TempData["Success"] = $"Chênh lệch giảm {diff} (Cập nhật '{product.ProductName}' từ {oldQty} xuống {newQty}).";
                }
                else
                {
                    TempData["Success"] = $"Không chênh lệch (Sản phẩm '{product.ProductName}' vẫn giữ nguyên là {oldQty}).";
                }

                // Cập nhật CSDL
                product.Quantity = newQty;
                db.SaveChanges();
            }

            // Sau khi cập nhật, văng về trang danh sách (Index) để xem tình trạng mới[cite: 9]
            return RedirectToAction("Index");
        }
        // 1. Mở trang Kiểm kê đồng thời
        public ActionResult KiemKeDongThoi()
        {
            // Nạp danh sách sản phẩm vào ViewBag để View tạo Dropdown chọn
            ViewBag.ProductList = new SelectList(db.Products, "ProductID", "ProductName");
            return View();
        }

        // 2. Xử lý Transaction cập nhật 2 sản phẩm cùng lúc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KiemKeDongThoi(int id1, int qty1, int id2, int qty2)
        {
            // Giữ lại danh sách Dropdown lỡ có lỗi bị văng lại trang cũ
            ViewBag.ProductList = new SelectList(db.Products, "ProductID", "ProductName");

            // VALIDATION 1: Chặn ID trùng nhau (Test 5.3)
            if (id1 == id2)
            {
                TempData["Error"] = "Lỗi: Không được chọn cùng 1 sản phẩm để kiểm kê!";
                return View();
            }

            // VALIDATION 2: Chặn số lượng âm (Test 5.1)[cite: 9, 10]
            if (qty1 < 0 || qty2 < 0)
            {
                TempData["Error"] = "Lỗi: Số lượng thực tế không hợp lệ (phải >= 0)!";
                return View();
            }

            // BẮT ĐẦU TRANSACTION[cite: 9]
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Tìm 2 sản phẩm trong DB
                    var p1 = db.Products.Find(id1);
                    var p2 = db.Products.Find(id2);

                    if (p1 != null && p2 != null)
                    {
                        // Gán số lượng mới
                        p1.Quantity = qty1;
                        p2.Quantity = qty2;

                        // Lưu vào CSDL
                        db.SaveChanges();

                        // NẾU TẤT CẢ TRÓT LỌT -> XÁC NHẬN LƯU (COMMIT)[cite: 9]
                        transaction.Commit();

                        TempData["Success"] = $"Đã cập nhật đồng thời: {p1.ProductName} thành {qty1}, và {p2.ProductName} thành {qty2}.";
                        return RedirectToAction("Index"); // Văng về trang danh sách
                    }

                    TempData["Error"] = "Lỗi: Không tìm thấy sản phẩm!";
                    return View();
                }
                catch
                {
                    // NẾU CÓ BẤT KỲ LỖI GÌ XẢY RA LÚC LƯU -> HỦY BỎ TẤT CẢ (ROLLBACK)[cite: 9]
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi hệ thống. Đã hủy bỏ toàn bộ giao dịch!";
                    return View();
                }
            }

        }
        // 1. Mở trang Thêm sản phẩm mới (GET)
        public ActionResult Create()
        {
            return View();
        }

        // 2. Nhận dữ liệu để Lưu (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Dùng Bind để chỉ nhận 4 cột (ProductID tự tăng trong DB nên không cần nhận)
        public ActionResult Create([Bind(Include = "ProductName,Category,Price,Quantity")] Product product)
        {
            // BƯỚC 1: Bắt lỗi rỗng và lỗi số âm (Đáp ứng Test 6.1)
            if (string.IsNullOrWhiteSpace(product.ProductName))
                ModelState.AddModelError("ProductName", "Tên sản phẩm không được để trống!");

            if (string.IsNullOrWhiteSpace(product.Category))
                ModelState.AddModelError("Category", "Danh mục không được để trống!");

            if (product.Price <= 0)
                ModelState.AddModelError("Price", "Đơn giá phải lớn hơn 0!");

            if (product.Quantity < 0)
                ModelState.AddModelError("Quantity", "Số lượng không được âm!");

            // Nếu các điều kiện cơ bản đã qua, ta xét tiếp trùng tên
            if (ModelState.IsValid)
            {
                // BƯỚC 2: Chuẩn hóa chuỗi (Cắt khoảng trắng 2 đầu và In thường)[cite: 9]
                string checkName = product.ProductName.Trim().ToLower();

                // Kiểm tra xem trong DB đã có tên này chưa
                bool isExist = db.Products.Any(p => p.ProductName.Trim().ToLower() == checkName);

                // NẾU TRÙNG TÊN (Đáp ứng Test 6.2)
                if (isExist)
                {
                    ModelState.AddModelError("ProductName", "Lỗi: Tên sản phẩm này đã tồn tại trong kho!");
                    return View(product); // Trả lại form kèm báo lỗi
                }

                // BƯỚC 3: Nếu hợp lệ hoàn toàn thì Lưu vào CSDL (Đáp ứng Test 6.3)[cite: 9, 10]
                // Gán lại tên đã cắt khoảng trắng cho gọn gàng
                product.ProductName = product.ProductName.Trim();
                product.Category = product.Category.Trim();

                db.Products.Add(product);
                db.SaveChanges(); // Entity Framework sẽ tự động sinh ProductID mới

                TempData["Success"] = "Thêm sản phẩm mới thành công!";
                return RedirectToAction("Index"); // Văng về trang danh sách
            }

            // Trả về View nếu có lỗi ở Bước 1
            return View(product);
        }
        // Mở trang Thống kê
        public ActionResult ThongKe()
        {
            // 1. Tính tổng số lượng sản phẩm (đầu mục sản phẩm)
            ViewBag.TongSoSP = db.Products.Count();

            // 2. Tính số lượng sản phẩm đang hết hàng (Quantity == 0)
            ViewBag.SPHetHang = db.Products.Count(p => p.Quantity == 0);

            // 3. Tính tổng giá trị hàng tồn = SUM(Price * Quantity)[cite: 9]
            // Lưu ý: Phải ép kiểu (decimal?) để đề phòng trường hợp bảng rỗng bị lỗi báo null
            ViewBag.TongGiaTri = db.Products.Sum(p => (decimal?)(p.Price * p.Quantity)) ?? 0;

            return View();
        }
    }
}