// UI 
const UI = {
    elements: {
        loginSection: null,
        dashboardSection: null,
        loading: null,
    },
    
    init() {
        // get DOM elements
        this.elements = {
            loginSection: document.getElementById('loginSection'),
            dashboardSection: document.getElementById('dashboardSection'),
            loading: document.getElementById('loading'),
            loginError: document.getElementById('loginError'),
            userInfo: document.getElementById('userInfo'),
            managerSection: document.getElementById('managerSection'),
            inventoryData: document.getElementById('inventoryData'),
            salesMessage: document.getElementById('salesMessage'),
            reorderData: document.getElementById('reorderData'),
            abcData: document.getElementById('abcData')
        };
        
        this.setupEventListeners();
    },
    
    setupEventListeners() {
        // login form
        document.getElementById('loginForm').addEventListener('submit', this.handleLogin.bind(this));
        
        // demo user buttons
        document.querySelectorAll('.demo-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                document.getElementById('username').value = e.target.dataset.user;
                document.getElementById('password').value = e.target.dataset.pass;
            });
        });
        
        // logout button
        document.getElementById('logoutBtn').addEventListener('click', () => Auth.logout());
        
        // Dashboard buttons 
        document.getElementById('salesForm').addEventListener('submit', this.handleSaleSubmit.bind(this));
        document.getElementById('loadReordersBtn').addEventListener('click', this.loadReorderRecommendations.bind(this));
        document.getElementById('loadAbcBtn').addEventListener('click', this.loadAbcAnalysis.bind(this));
        
        // inventory load button 
        document.addEventListener('click', (e) => {
            if (e.target && e.target.id === 'loadInventoryBtn') {
                this.loadInventory();
            }
        });
    },
    
    showLoading(show = true) {
        this.elements.loading.classList.toggle('d-none', !show);
    },
    
    showLogin() {
        this.elements.loginSection.classList.remove('d-none');
        this.elements.dashboardSection.classList.add('d-none');
        this.elements.loginError.classList.add('d-none');
    },
    
    showDashboard(user) {
        this.elements.loginSection.classList.add('d-none');
        this.elements.dashboardSection.classList.remove('d-none');
        
        this.elements.userInfo.textContent = `${user.username} (${user.role}) - Store: ${user.storeId || 'Any'}`;
        
        // show manager sections 
        if (user.role === 'StoreManager') {
            this.elements.managerSection.classList.remove('d-none');
        } else {
            this.elements.managerSection.classList.add('d-none');
        }

        // setup UI for different user types
        this.setupClientUI(user);
    },

    setupClientUI(user) {
        const inventoryControlsContainer = document.getElementById('inventoryControlsContainer');
        const salesStoreContainer = document.getElementById('salesStoreContainer');
        
        if (user.role === 'Client') {
            // setup inventory section for client users
            inventoryControlsContainer.innerHTML = `
                <div class="d-flex gap-2 align-items-center">
                    <input type="number" id="storeIdInput" class="form-control form-control-sm" 
                           placeholder="Store ID" min="1" style="width: 100px;">
                    <button class="btn btn-primary btn-sm" id="loadInventoryBtn">Load Inventory</button>
                </div>
            `;

            // Setup sales form for client users
            salesStoreContainer.classList.remove('d-none');
            
            document.getElementById('productIdContainer').className = 'col-md-3';
            document.getElementById('quantityContainer').className = 'col-md-3';
            document.getElementById('submitContainer').className = 'col-md-3';
        } else {
            // setup for other users 
            inventoryControlsContainer.innerHTML = `
                <button class="btn btn-primary btn-sm" id="loadInventoryBtn">Load Inventory</button>
            `;
            
            // hide store input and reset column sizes
            salesStoreContainer.classList.add('d-none');
            document.getElementById('productIdContainer').className = 'col-md-4';
            document.getElementById('quantityContainer').className = 'col-md-4';
            document.getElementById('submitContainer').className = 'col-md-4';
        }
    },
    
    showError(elementId, message) {
        const element = document.getElementById(elementId);
        element.innerHTML = `<div class="alert alert-danger">${message}</div>`;
        element.classList.remove('d-none');
    },
    
    showSuccess(elementId, message) {
        const element = document.getElementById(elementId);
        element.innerHTML = `<div class="alert alert-success">${message}</div>`;
    },
    
    async handleLogin(e) {
        e.preventDefault();
        
        const username = document.getElementById('username').value;
        const password = document.getElementById('password').value;
        const loginBtn = document.getElementById('loginBtn');
        
        loginBtn.disabled = true;
        loginBtn.textContent = 'Logging in...';
        this.elements.loginError.classList.add('d-none');
        
        const result = await Auth.login(username, password);
        
        if (result.success) {
            this.showDashboard(result.user);
        } else {
            this.elements.loginError.textContent = result.message;
            this.elements.loginError.classList.remove('d-none');
        }
        
        loginBtn.disabled = false;
        loginBtn.textContent = 'Login';
    },
    
    async loadInventory() {
        const user = Auth.getUser();
        let storeId;

        if (user.role === 'Client') {
            const storeInput = document.getElementById('storeIdInput');
            
            if (!storeInput) {
                this.elements.inventoryData.innerHTML = '<div class="alert alert-warning">Store input not found. Please try refreshing.</div>';
                return;
            }
            
            storeId = parseInt(storeInput.value);
            
            if (!storeId || storeId < 1) {
                this.elements.inventoryData.innerHTML = '<div class="alert alert-warning">Please enter a valid Store ID.</div>';
                return;
            }
        } else {
            if (!user.storeId) {
                this.elements.inventoryData.innerHTML = '<div class="alert alert-warning">No store assigned to this user.</div>';
                return;
            }
            storeId = user.storeId;
        }
        
        this.showLoading(true);
        
        try {
            
            const data = await API.getInventory(storeId);
            
            if (data.success && data.data) {
                this.renderInventoryTable(data.data);
            } else {
                this.elements.inventoryData.innerHTML = `<div class="alert alert-danger">Error: ${data.message}</div>`;
            }
        } catch (error) {
            this.elements.inventoryData.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
        } finally {
            this.showLoading(false);
        }
    },
    
    renderInventoryTable(inventory) {
        if (!inventory.length) {
            this.elements.inventoryData.innerHTML = '<div class="alert alert-info">No inventory data found.</div>';
            return;
        }
        
        const table = `
            <div class="table-responsive">
                <table class="table table-striped table-hover">
                    <thead class="table-dark">
                        <tr>
                            <th>ID</th>
                            <th>SKU</th>
                            <th>Product</th>
                            <th>Category</th>
                            <th>Stock</th>
                            <th>Min Stock</th>
                            <th>Price</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${inventory.map(item => `
                            <tr>
                                <td>${item.productId}</td>
                                <td><code>${item.sku}</code></td>
                                <td>${item.productName}</td>
                                <td><span class="badge bg-secondary">${item.category}</span></td>
                                <td><strong>${item.currentStock}</strong></td>
                                <td>${item.minStockLevel}</td>
                                <td>$${item.price.toFixed(2)}</td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;
        
        this.elements.inventoryData.innerHTML = table;
    },
    
    async handleSaleSubmit(e) {
        e.preventDefault();
        
        const user = Auth.getUser();
        let storeId;

        const salesStoreInput = document.getElementById('salesStoreId');
        const salesStoreContainer = document.getElementById('salesStoreContainer');
    
        if (user.role === 'Client') {
            salesStoreInput.setAttribute('required', 'true');
            
            if (!salesStoreInput.value || parseInt(salesStoreInput.value) < 1) {
                this.showError('salesMessage', 'Please enter a valid Store ID.');
                return;
            }
            
            storeId = parseInt(salesStoreInput.value);
            
        } else {
            // managers and operators, remove required and use user's store
            salesStoreInput.removeAttribute('required');
            
            if (!user.storeId) {
                this.showError('salesMessage', 'No store assigned to this user.');
                return;
            }
            storeId = user.storeId;
        }
        
        const productId = parseInt(document.getElementById('productId').value);
        const quantity = parseInt(document.getElementById('quantity').value);
        const submitBtn = e.target.querySelector('button[type="submit"]');
        
        submitBtn.disabled = true;
        submitBtn.textContent = 'Recording...';
        this.elements.salesMessage.innerHTML = '';
        
        try {
            const saleData = {
                storeId: storeId,
                productId: productId,
                quantity: quantity,
                saleDate: new Date().toISOString()
            };
            
            const data = await API.recordSale(saleData);
            
            if (data.success) {
                this.showSuccess('salesMessage', 'Sale recorded successfully!');
                document.getElementById('salesForm').reset();
            } else {
                this.showError('salesMessage', data.message);
            }
        } catch (error) {
            this.showError('salesMessage', error.message);
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = 'Record Sale';
        }
    },
    
    async loadReorderRecommendations() {
        const user = Auth.getUser();
        if (!user || !user.storeId) return;
        
        this.showLoading(true);
        
        try {
            const data = await API.getReorderRecommendations(user.storeId);
            
            if (data.success && data.data) {
                this.renderReorderTable(data.data);
            } else {
                this.elements.reorderData.innerHTML = `<div class="alert alert-danger">Error: ${data.message}</div>`;
            }
        } catch (error) {
            this.elements.reorderData.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
        } finally {
            this.showLoading(false);
        }
    },
    
    renderReorderTable(recommendations) {
        if (!recommendations.length) {
            this.elements.reorderData.innerHTML = '<div class="alert alert-info">No reorder recommendations at this time.</div>';
            return;
        }
        
        const table = `
            <div class="table-responsive">
                <table class="table table-striped">
                    <thead class="table-dark">
                        <tr>
                            <th>Product</th>
                            <th>SKU</th>
                            <th>Category</th>
                            <th>Current Stock</th>
                            <th>Min Stock</th>
                            <th>Recommended Qty</th>
                            <th>Priority</th>
                            <th>Cost</th>
                            <th>Generated</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${recommendations.map(rec => `
                            <tr>
                                <td><strong>${rec.productName}</strong></td>
                                <td><code>${rec.sku}</code></td>
                                <td><span class="badge bg-secondary">${rec.category}</span></td>
                                <td>${rec.currentStock}</td>
                                <td>${rec.minStockLevel}</td>
                                <td><strong class="text-primary">${rec.recommendedQuantity}</strong></td>
                                <td><span class="badge badge-priority-${rec.priority.toLowerCase()}">${rec.priority}</span></td>
                                <td><strong>$${rec.estimatedCost.toFixed(2)}</strong></td>
                                <td><small>${new Date(rec.generatedDate).toLocaleDateString()}</small></td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
                <div class="mt-3">
                    <div class="row">
                        <div class="col-md-6">
                            <strong>Total Recommendations:</strong> ${recommendations.length}
                        </div>
                        <div class="col-md-6 text-end">
                            <strong>Estimated Total Cost:</strong> 
                            <span class="text-success">$${recommendations.reduce((sum, rec) => sum + rec.estimatedCost, 0).toFixed(2)}</span>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        this.elements.reorderData.innerHTML = table;
    },
    
    async loadAbcAnalysis() {
        const user = Auth.getUser();
        if (!user || !user.storeId) return;
        
        this.showLoading(true);
        
        try {
            const data = await API.getAbcAnalysis(user.storeId);
            
            if (data.success && data.data) {
                this.renderAbcAnalysis(data.data);
                this.createAbcChart(data.data);
            } else {
                this.elements.abcData.innerHTML = `<div class="alert alert-danger">Error: ${data.message}</div>`;
            }
        } catch (error) {
            this.elements.abcData.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
        } finally {
            this.showLoading(false);
        }
    },
    
    renderAbcAnalysis(analysis) {
        if (!analysis.length) {
            this.elements.abcData.innerHTML = '<div class="alert alert-info">No ABC analysis data available.</div>';
            return;
        }
        
        const table = `
            <div class="table-responsive">
                <table class="table table-sm table-striped">
                    <thead class="table-dark">
                        <tr>
                            <th>Product</th>
                            <th>SKU</th>
                            <th>Category</th>
                            <th>ABC Class</th>
                            <th>Revenue</th>
                            <th>Revenue %</th>
                            <th>Cumulative %</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${analysis.map(item => `
                            <tr>
                                <td>${item.productName}</td>
                                <td><code>${item.sku}</code></td>
                                <td><span class="badge bg-info">${item.category}</span></td>
                                <td><span class="badge badge-abc-${item.abcCategory.toLowerCase()}">${item.abcCategory}</span></td>
                                <td><strong>$${item.totalRevenue.toFixed(2)}</strong></td>
                                <td>${item.revenuePercentage.toFixed(1)}%</td>
                                <td>${item.cumulativePercentage.toFixed(1)}%</td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;
        
        this.elements.abcData.innerHTML = table;
    },
    
    createAbcChart(data) {
        if (!data.length) return;
        
        const counts = data.reduce((acc, item) => {
            acc[item.abcCategory] = (acc[item.abcCategory] || 0) + 1;
            return acc;
        }, {});
        
        const ctx = document.getElementById('abcChart').getContext('2d');
        
        if (window.abcChartInstance) {
            window.abcChartInstance.destroy();
        }
        
        window.abcChartInstance = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: Object.keys(counts).map(key => `Category ${key}`),
                datasets: [{
                    data: Object.values(counts),
                    backgroundColor: ['#dc3545', '#ffc107', '#198754'],
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: { position: 'bottom' }
                }
            }
        });
    }
};
