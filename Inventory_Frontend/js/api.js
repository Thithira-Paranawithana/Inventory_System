// APIs 
const API = {
    // API call 
    async call(endpoint, options = {}) {
        const token = Auth.getToken();
        
        const config = {
            headers: {
                'Content-Type': 'application/json',
                ...(token && { 'Authorization': `Bearer ${token}` }),
                ...options.headers
            },
            ...options
        };
        
        try {
            const response = await fetch(`${CONFIG.API_BASE}${endpoint}`, config);
            
            if (response.status === 401) {
                Auth.logout();
                throw new Error('Session expired. Please login again.');
            }
            
            return response;
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    },

    // authentication 
    async login(username, password) {
        const response = await this.call('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password })
        });
        return response.json();
    },

    async logout() {
        try {
            await this.call('/auth/logout', { method: 'POST' });
        } catch (error) {
            console.log('Logout API call failed, but continuing with local logout');
        }
    },

    // getInventory 
    async getInventory(storeId) {
        const response = await this.call(`/inventory/${storeId}`);
        return response.json();
    },

    // post Sales
    async recordSale(saleData) {
        const response = await this.call('/sales/transaction', {
            method: 'POST',
            body: JSON.stringify(saleData)
        });
        return response.json();
    },

    // reorder-recommendations
    async getReorderRecommendations(storeId) {
        const response = await this.call(`/algorithms/reorder-recommendations/${storeId}`);
        return response.json();
    },

    // abc-analysis
    async getAbcAnalysis(storeId) {
        const response = await this.call(`/algorithms/abc-analysis/${storeId}`);
        return response.json();
    }
};
