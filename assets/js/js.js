document.addEventListener("DOMContentLoaded", function () {
    var buttons = document.querySelectorAll(".btnthem");

    // Load products from localStorage
    loadProductsFromLocalStorage();

    buttons.forEach(function (button) {
        button.addEventListener("click", function () {
            var id = this.getAttribute("data-id");
            var name = this.getAttribute("data-name");
            var price = parseFloat(this.getAttribute("data-price"));
            var dvt = this.getAttribute("data-dvt");
            var loai = this.getAttribute("data-loai");
            var image = this.getAttribute("data-image");
            var quantity = 1;

            // Check if product already exists in the list
            if (document.querySelector(`#selected-products [data-id='${id}']`)) {
                alert("Sản phẩm này đã được thêm vào.");
                return;
            }

            // Create a new div to display the selected product
            var selectedProductDiv = document.createElement("div");
            selectedProductDiv.className = "d-flex justify-content-between align-items-center mb-2";
            selectedProductDiv.setAttribute("data-id", id);
            selectedProductDiv.innerHTML = `
                <img src="/Images/${image}" style="width:45px; height:45px" />
                <span class="product-name">${name}</span>
                <span class="unit-price">${price.toFixed(2)} VND</span>
                <div class="d-flex align-items-center gap-2">
                    <button class="btn btn-secondary btn-sm" onclick="updateQuantity(this, -1)">-</button>
                    <input type="text" class="form-control text-center quantity-input" value="${quantity}" style="width: 3rem;">
                    <button class="btn btn-secondary btn-sm" onclick="updateQuantity(this, 1)">+</button>
                </div>
                <span class="total-price">${(price * quantity).toFixed(2)} VND</span>
                <button class="btn btn-danger btn-sm" onclick="removeProduct(this)">Xóa</button>
            `;

            document.getElementById("selected-products").appendChild(selectedProductDiv);
            updateSummary();
            saveProductsToLocalStorage();
        });
    });

    document.getElementById("amount-received").addEventListener("input", calculateChange);




    // Xử lý sự kiện nhấn nút "Tạo đơn hàng"
    document.getElementById("create-order").addEventListener("click", function () {
        var products = [];
        var productElements = document.querySelectorAll("#selected-products .d-flex.justify-content-between.align-items-center.mb-2");

        productElements.forEach(function (productElement) {
            var id = productElement.getAttribute("data-id");
            var name = productElement.querySelector(".product-name").textContent;
            var price = parseFloat(productElement.querySelector(".unit-price").textContent);
            var quantity = parseInt(productElement.querySelector("input").value);

            products.push({ IdSanPham: id, TenSanPham: name, Gia: price, SoLuong: quantity });
        });
        console.log("Products to order:", products);
        if (products.length === 0) {
            alert("Không có sản phẩm nào được chọn.");
            return;
        }

        var totalPrice = parseFloat(document.getElementById("total-price").textContent);
        var amountReceived = parseFloat(document.getElementById("amount-received").value) || 0;
        var changeAmount = amountReceived - totalPrice;

        var orderData = {
            SanPhams: products,
            PhaiTra: totalPrice,
            TienTraLai: changeAmount
        };
        console.log("Order data:", orderData);
        console.log("URL: ", '@Url.Action("CreateOrder", "BanHang")');

        $.ajax({
            url: '/BanHang/CreateOrder',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(orderData),
            success: function (response) {
                if (response.success) {
                    alert(response.message);

                    
                    //$('#productList').html(response);

                    
                    //document.getElementById("selected-products").innerHTML = '';
                    //updateSummary();
                    //localStorage.removeItem("selectedProducts");

                    //location.reload();


                        

                    // Ẩn các nút "Thêm"
                    buttons.forEach(function (button) {
                        button.style.display = "none";
                    });



                    document.getElementById("create-order").style.display = "none";
                    document.getElementById("complete-order").style.display = "block";
                    document.getElementById("print-receipt").style.display = "block";

                    // Xử lý sự kiện khi nhấn vào nút "Hoàn thành"
                    document.getElementById("complete-order").addEventListener("click", function () {
                        document.getElementById("selected-products").innerHTML = '';
                        updateSummary();
                        localStorage.removeItem("selectedProducts");
                        buttons.forEach(function (button) {
                            button.style.display = "block";
                        });
                        location.reload();
                    });

                    // Xử lý sự kiện khi nhấn vào nút "In hóa đơn"
                    document.getElementById("print-receipt").addEventListener("click", function () {
                        // Gọi hàm in hóa đơn từ controller
                        window.location.href = `/BanHang/PrintReceipt?orderId=${response.orderId}`;
                    });

                } else {
                    alert(response.message);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert("Đã có lỗi xảy ra. Vui lòng thử lại.");
                console.error("AJAX error:", textStatus, errorThrown);
                console.error("Response text:", jqXHR.responseText);
            }
        });


    });
});

function updateQuantity(button, change) {
    var quantityInput = button.parentNode.querySelector("input");
    var quantity = parseInt(quantityInput.value) || 0;
    quantity += change;
    if (quantity < 1) quantity = 1;
    quantityInput.value = quantity;

    // Update total price
    var selectedProductDiv = button.closest(".d-flex.justify-content-between.align-items-center.mb-2");
    var unitPriceSpan = selectedProductDiv.querySelector(".unit-price");
    var totalPriceSpan = selectedProductDiv.querySelector(".total-price");
    var unitPrice = parseFloat(unitPriceSpan.textContent);
    totalPriceSpan.textContent = (unitPrice * quantity).toFixed(2) + " VND";

    // Update summary and save to localStorage
    updateSummary();
    saveProductsToLocalStorage();
}

function removeProduct(button) {
    button.closest(".d-flex.justify-content-between.align-items-center.mb-2").remove();
    updateSummary();
    saveProductsToLocalStorage();
}

function updateSummary() {
    var totalQuantity = 0;
    var totalPrice = 0;
    var products = document.querySelectorAll("#selected-products .d-flex.justify-content-between.align-items-center.mb-2");

    products.forEach(function (product) {
        var quantity = parseInt(product.querySelector("input").value) || 0;
        var totalPriceSpan = product.querySelector(".total-price");
        var price = parseFloat(totalPriceSpan.textContent);
        totalQuantity += quantity;
        totalPrice += price;
    });

    document.getElementById("total-quantity").textContent = totalQuantity;
    document.getElementById("total-price").textContent = totalPrice.toFixed(2) + " VND";

    calculateChange();
}

function calculateChange() {
    var totalPrice = parseFloat(document.getElementById("total-price").textContent);
    var amountReceived = parseFloat(document.getElementById("amount-received").value) || 0;
    var changeAmount = amountReceived - totalPrice;
    document.getElementById("change-amount").textContent = changeAmount.toFixed(2) + " VND";
}

function saveProductsToLocalStorage() {
    var products = [];
    var productElements = document.querySelectorAll("#selected-products .d-flex.justify-content-between.align-items-center.mb-2");

    productElements.forEach(function (productElement) {
        var id = productElement.getAttribute("data-id");
        var name = productElement.querySelector(".product-name").textContent;
        var price = parseFloat(productElement.querySelector(".unit-price").textContent);
        var quantity = parseInt(productElement.querySelector("input").value);
        var image = productElement.querySelector("img").getAttribute("src");

        products.push({ id, name, price, quantity, image });
    });

    localStorage.setItem("selectedProducts", JSON.stringify(products));
}

function loadProductsFromLocalStorage() {
    var products = JSON.parse(localStorage.getItem("selectedProducts")) || [];

    products.forEach(function (product) {
        var selectedProductDiv = document.createElement("div");
        selectedProductDiv.className = "d-flex justify-content-between align-items-center mb-2";
        selectedProductDiv.setAttribute("data-id", product.id);
        selectedProductDiv.innerHTML = `
            <img src="${product.image}" width="45" />
            <span class="product-name">${product.name}</span>
            <span class="unit-price">${product.price.toFixed(2)} VND</span>
            <div class="d-flex align-items-center gap-2">
                <button class="btn btn-secondary btn-sm" onclick="updateQuantity(this, -1)">-</button>
                <input type="text" class="form-control text-center quantity-input" value="${product.quantity}" style="width: 3rem;">
                <button class="btn btn-secondary btn-sm" onclick="updateQuantity(this, 1)">+</button>
            </div>
            <span class="total-price">${(product.price * product.quantity).toFixed(2)} VND</span>
            <button class="btn btn-danger btn-sm" onclick="removeProduct(this)">Xóa</button>
        `;

        document.getElementById("selected-products").appendChild(selectedProductDiv);
    });

    updateSummary();
}








var currentChartType = 'bar';

document.addEventListener("DOMContentLoaded", function () {
    loadRevenueData('month'); // Load dữ liệu mặc định
});

function loadRevenueData(period) {
    var date;
    if (period === 'day') {
        date = $('#specificDate').val();
    } else if (period === 'month') {
        date = $('#specificMonth').val();
    } else if (period === 'year') {
        date = $('#specificYear').val();
    }

    $.ajax({
        url: '/Home/GetRevenueData',
        type: 'GET',
        data: { period: period, date: date },
        success: function (revenueData) {
            updateChart(revenueData, period);
        },
        error: function () {
            alert("Đã có lỗi xảy ra. Vui lòng thử lại.");
        }
    });
}

function loadSpecificRevenueData(period) {
    loadRevenueData(period);
}

function updateChart(revenueData, period) {
    var ctx = document.getElementById('revenueChart').getContext('2d');

    var labels = [];
    var data = [];

    if (period === 'day') {
        labels.push(revenueData.Label);
        data.push(revenueData.TotalRevenue);
    } else if (period === 'month') {
        var daysInMonth = new Date(revenueData.Year, revenueData.Month, 0).getDate();
        for (var i = 1; i <= daysInMonth; i++) {
            labels.push(i + "/" + revenueData.Month + "/" + revenueData.Year);
            var dayData = revenueData.Data ? revenueData.Data.find(d => d.Day === i) : null;
            data.push(dayData ? dayData.TotalRevenue : 0);
        }
    } else if (period === 'year') {
        for (var i = 1; i <= 12; i++) {
            labels.push(i + "/" + revenueData.Year);
            var monthData = revenueData.Data ? revenueData.Data.find(d => d.Month === i) : null;
            data.push(monthData ? monthData.TotalRevenue : 0);
        }
    }

    if (window.myChart) {
        window.myChart.destroy();
    }

    window.myChart = new Chart(ctx, {
        type: currentChartType,
        data: {
            labels: labels,
            datasets: [{
                label: 'Doanh thu',
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                borderColor: 'rgba(75, 192, 192, 1)',
                data: data
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    title: {
                        text: 'Thời gian',
                        display: true
                    }
                },
                y: {
                    title: {
                        text: 'Doanh thu',
                        display: true
                    }
                }
            }
        }
    });
}