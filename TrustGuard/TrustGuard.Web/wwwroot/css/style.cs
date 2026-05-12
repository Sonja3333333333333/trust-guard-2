
:root {
    --tg-bg-page: #f4f7f9; 
    --tg - text - main: #212529;
    --tg - card - bg: #ffffff;
    --tg - border - color: #e2e8f0; /* Світло-сіра рамка замість чорної */
}

html {
    font-size: 14px;
position: relative;
min - height: 100 %;
}

@media(min - width: 768px) {
    html {
        font - size: 16px;
    }
}

body {
    margin-bottom: 60px;
background - color: var(--tg - bg - page);
color: var(--tg - text - main);
font - family: 'Segoe UI', system - ui, -apple - system, sans - serif;
}

.tg - card {
    background - color: var(--tg - card - bg);
border: 1px solid var(--tg - border - color);
    border - radius: 12px;
    box - shadow: 0 4px 6px - 1px rgba(0, 0, 0, 0.05), 0 2px 4px - 1px rgba(0, 0, 0, 0.03);
overflow: hidden;
}

.custom - scrollbar::- webkit - scrollbar {
width: 6px;
}
.custom - scrollbar::- webkit - scrollbar - track {
background: transparent;
}
.custom - scrollbar::- webkit - scrollbar - thumb {
background: #cbd5e1; 
    border - radius: 10px;
}
.custom - scrollbar::- webkit - scrollbar - thumb:hover {
    background: #94a3b8; 
}