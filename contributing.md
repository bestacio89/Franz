# 🤝 Contributing to Franz-Powered API

First off — thanks for wanting to contribute 🙌
This project is not just code; it’s **Architecture-as-Code**.
That means contributions must **comply with the rules** — or they will be rejected by design.

---

## 🚦 Ground Rules

1. **Architecture is Law**

   * Command handlers must end with `CommandHandler`.
   * Query handlers must end with `QueryHandler`.
   * DTOs must end with `Dto`.
   * Repositories must implement the correct lifetime interface.
   * Break a rule → architecture tests fail → no merge.

2. **Tests Never Lie**

   * PRs **must include tests** (unit + integration where relevant).
   * Architecture tests run automatically and will block PRs if you drift.

3. **No Spaghetti Allowed**

   * “Quick hacks” are not accepted.
   * If it looks like duct tape, DI will pretend your code doesn’t exist.

---

## 📦 Setting up your Dev Environment

This repo ships with **IDE-as-Code**:

1. Clone the repo.

2. Open in **VS Code**.

3. Install recommended extensions.

4. Run:

   ```bash
   dotnet restore
   dotnet build
   dotnet test
   ```

5. For infra testing:

   ```bash
   cd Infrastructure/Terraform-GCP
   terraform init -backend=false
   terraform validate
   ```

---

## 🔄 Workflow

1. **Fork** this repo.

2. **Create a feature branch**:

   ```bash
   git checkout -b feat/my-awesome-feature
   ```

3. **Commit with discipline**:

   * Use [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/).
   * Example:

     * `[Feat] Added user query handler with tests`
     * `[Fix] Corrected Kafka consumer config`

4. **Push & PR**:

   * Target `develop` branch.
   * Ensure PR description includes:

     * **What** changed
     * **Why** it’s needed
     * **Tests** included

---

## 🔒 CI/CD

* Every PR triggers:

  * ✅ Build & Tests
  * ✅ Architecture Rules (ArchUnitNET)
  * ✅ Docker Build (multi-stage)
  * ✅ Terraform/Bicep validation (IaC discipline)

* Only maintainers can trigger **infrastructure apply jobs**.

---

## 🦉 Our Contribution Creed

> *“This is not a democracy — the rules enforce themselves.”*

* Your creativity is welcome.
* Your spaghetti is not.
* If your PR drifts, the failing suite will slap you back into line.

---

## 🙏 Code of Conduct

Respect each other. This project is technical, not political.
Debates about patterns are welcome — but **the tests decide, not opinions**.

---