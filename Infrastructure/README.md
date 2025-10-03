# Infrastructure Modules (Bicep)

This folder contains reusable **Bicep modules** for deploying common infrastructure components in Azure.
Each module is designed to be **composable, parameter-driven, and environment-agnostic**, enabling flexible deployment across **dev, preprod, and prod** without rewriting templates.

---

## 📂 Modules Overview

### 🔹 **Database**

* **`Single/single-db.bicep`**
  Deploys a **single database instance**.
  Supports multiple engines (SQL Server, MySQL, PostgreSQL, MariaDB, Oracle-as-VM).
  Ideal for services that need a **dedicated datastore**.

* **`Multi/multi-db.bicep`**
  Deploys **multiple database backends** in parallel (polyglot persistence).
  Useful when a system requires different storage types (e.g., PostgreSQL + Cosmos DB).
  Perfect for **DDD storage patterns** or enterprise projects with hybrid storage needs.

### 🔹 **Kafka**

* **`kafka.bicep`**
  Deploys a Kafka cluster with all required configs.
  Designed for **event-driven microservices**.
  Can be combined with `multi-db` to support **CQRS + Event Sourcing**.

### 🔹 **Key Vault**

* **`keyvault.bicep`**
  Deploys an Azure Key Vault.
  Ensures **secure storage of secrets, keys, and certificates**.
  Can be consumed by any other module (databases, Kafka, APIs).

---

## ⚙️ Main Orchestrator

* **`main.bicep`**
  The **entry point** that wires modules together.
  Reads parameters (e.g., `entityStorageType`, `useKafka`, etc.) and provisions the right modules.
  Keeps deployments **uniform, repeatable, and environment-aware**.

---

## 🚀 Usage

### Example: Deploy a Single DB with Key Vault

```bash
az deployment group create \
  --resource-group my-rg \
  --template-file main.bicep \
  --parameters entityStorageType=SQLServer useKafka=false
```

### Example: Deploy Multi-DB + Kafka

```bash
az deployment group create \
  --resource-group my-rg \
  --template-file main.bicep \
  --parameters entityStorageType=Multi useKafka=true
```

---

## 🔮 Why this structure?

✔ Future-proof → add new modules without refactor.
✔ Scalable → use Single DB for small apps, Multi DB for enterprise systems.
✔ Secure → native Key Vault integration.
✔ Cloud-ready → easily portable patterns for AWS (Terraform) and GCP.

