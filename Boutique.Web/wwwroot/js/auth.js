
const tabs = document.querySelectorAll('.tab');
const forms = document.querySelectorAll('.form-container');

    tabs.forEach(tab => {
    tab.addEventListener('click', () => {
        // Remove active class from all tabs and forms
        tabs.forEach(t => t.classList.remove('active'));
        forms.forEach(f => f.classList.remove('active'));

        // Add active class to the clicked tab and corresponding form
        tab.classList.add('active');
        document.getElementById(tab.dataset.tab).classList.add('active');
    });
    });
