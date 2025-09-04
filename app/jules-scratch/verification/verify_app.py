from playwright.sync_api import sync_playwright

with sync_playwright() as p:
    browser = p.chromium.launch()
    page = browser.new_page()
    page.goto("http://localhost:5173/src")
    page.screenshot(path="app/jules-scratch/verification/verification.png")
    browser.close()
