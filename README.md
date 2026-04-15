#  CSharpPortfolioProject

## Overview

This repository contains an automated testing project for a web application using **Playwright** (C#), BDD (Reqnroll/SpecFlow), NUnit, and the Page Object + Component Object patterns.  
The project currently implements UI tests, has a prepared structure for API tests, supports parallel execution and headless mode, CI/CD integration, and reporting in Allure.

---

## Project Structure

- **Attributes/** — Custom attributes for element lookup.
- **Components/** — Base and specific UI components (buttons, text fields, header, footer, search results).
- **Context/** — Test context storage (page, current Page Object).
- **Drivers/** — Playwright driver initialization and management.
- **Features/** — BDD scenario files (Gherkin) for UI and API.
- **Helpers/** — Utilities for element lookup, page factory.
- **Hooks/** — Hooks for environment setup and teardown before/after scenarios.
- **Pages/** — Page Objects for application pages.
- **Steps/** — Step definitions for BDD scenarios (UI and API).
- **Tests/** — Base classes for tests (NUnit).
- **API/** — Folder for API automation:
  - **Models/** — Request/response models for API.
  - **Clients/** — Classes for sending HTTP requests.
  - **Steps/** — Step definitions for API scenarios.
  - **Tests/** — (Optional) standalone API tests.

---

## Technologies Used

- **Playwright** — Browser automation (C#).
- **Reqnroll/SpecFlow** — BDD, Gherkin scenarios.
- **NUnit** — Test execution and organization.
- **FluentAssertions** — Modern assertions.
- **Headless mode** — Tests run without browser UI (default setting).
- **CI/CD** — Smoke tests run via YAML pipeline.
- **Allure** — Test reporting.
- **(Planned) Mocking** — For API and edge cases.

---

## How to Run Tests

1. **Install dependencies**  
   - .NET SDK (7.0+)
   - Playwright CLI:  
     ```
     pwsh bin/Debug/net7.0/playwright.ps1 install
     ```
   - All NuGet packages will be restored automatically during build.

2. **Run tests locally**  
   - UI tests:  
     ```
     dotnet test --filter Category=UI
     ```
   - API tests:  
     ```
     dotnet test --filter Category=API
     ```
   - All smoke tests 
     ```
     dotnet test --filter "Category=smoke"
     ```
   - By default, tests run in headless mode.

3. **Run in CI/CD**  
   - Smoke tests are triggered automatically via the `.yml` pipeline configuration.

4. **Enable Allure reporting**  
   - Add the `Allure.NUnit` or `Allure.Reqnroll` package.
   - After test execution, reports will be available in the `allure-results` folder.

---

## Example Scenarios

- **UI:**  
  - Site search with result verification.
  - Page navigation checks.
  - UI element validation (color, visibility, cursor).
- **API:**  
  - CRUD operations: create, update, delete records.
  - Validation checks, negative scenarios, response mocking
