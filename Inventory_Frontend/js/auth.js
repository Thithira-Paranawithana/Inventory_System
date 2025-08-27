// Authentication 
const Auth = {
    currentUser: null,
    
    // check if user is logged in
    isLoggedIn() {
        return !!this.getToken() && !!this.getUser();
    },
    
    // get token
    getToken() {
        return localStorage.getItem(CONFIG.TOKEN_KEY);
    },
    
    // get user
    getUser() {
        const userStr = localStorage.getItem(CONFIG.USER_KEY);
        return userStr ? JSON.parse(userStr) : null;
    },
    
    // set authentication data
    setAuth(token, userData) {
        localStorage.setItem(CONFIG.TOKEN_KEY, token);
        localStorage.setItem(CONFIG.USER_KEY, JSON.stringify(userData));
        this.currentUser = userData;
    },
    
    // clear authentication data
    clearAuth() {
        localStorage.removeItem(CONFIG.TOKEN_KEY);
        localStorage.removeItem(CONFIG.USER_KEY);
        this.currentUser = null;
    },
    
    // login 
    async login(username, password) {
        try {
            const data = await API.login(username, password);
            
            if (data.token) {
                const userData = {
                    username: data.username,
                    role: data.role,
                    storeId: data.storeId
                };
                
                this.setAuth(data.token, userData);
                return { success: true, user: userData };
            } else {
                return { success: false, message: data.message || 'Login failed' };
            }
        } catch (error) {
            return { success: false, message: error.message || 'Network error' };
        }
    },
    
    // logout 
    async logout() {
        try {
            await API.logout();
        } catch (error) {
            console.log('Logout API error:', error);
        } finally {
            this.clearAuth();
            UI.showLogin();
        }
    },
    
    // auth state initialize
    init() {
        this.currentUser = this.getUser();
        
        if (this.isLoggedIn()) {
            UI.showDashboard(this.currentUser);
        } else {
            UI.showLogin();
        }
    }
};
