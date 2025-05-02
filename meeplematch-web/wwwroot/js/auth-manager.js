document.addEventListener('DOMContentLoaded', function () {
    const token = sessionStorage.getItem('jwtToken');

    const loginTab = document.getElementById('loginTab');
    const logoutTab = document.getElementById('logoutTab');
    const usersTab = document.getElementById('usersTab');

    if (token) {
        try {
            const payloadBase64 = token.split('.')[1];
            const payloadJson = atob(payloadBase64);
            const payload = JSON.parse(payloadJson);

            const role = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

            if (role === "admin" && usersTab) {
                usersTab.style.display = 'block';
            }

            if (loginTab) loginTab.style.display = 'none';
            if (logoutTab) logoutTab.style.display = 'block';

        } catch (err) {
            console.error('Failed to decode token:', err);
            sessionStorage.removeItem('jwtToken');
        }
    } else {
        if (loginTab) loginTab.style.display = 'block';
        if (logoutTab) logoutTab.style.display = 'none';
    }

    const logoutLink = document.getElementById('logoutLink');
    if (logoutLink) {
        logoutLink.addEventListener('click', function (event) {
            event.preventDefault();
            sessionStorage.removeItem('jwtToken');
            window.location.href = '/Auth/Logout';
        });
    }
});
