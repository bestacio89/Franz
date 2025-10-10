# Terraform GCP Infrastructure Modules

This folder contains reusable **Terraform modules** for provisioning infrastructure on **Google Cloud Platform (GCP)**.
Each module is designed to be **modular, composable, and DevOps-ready**, enabling deployments across environments (dev, preprod, prod) with consistent pipelines.

---

## 📂 Modules Overview

### 🔹 **Cloud Run**

* **`cloudrun/`**
  Provisions a fully managed **Cloud Run service** for serverless workloads.
  Ideal for APIs, microservices, and lightweight event-driven apps.

### 🔹 **Database**

* **`database/`**
  Provisions GCP-managed databases (Cloud SQL, PostgreSQL, MySQL, etc.).
  Exposes outputs (connection strings, instance IDs) for consumption by apps.

### 🔹 **GKE (Google Kubernetes Engine)**

* **`gke/`**
  Creates a GKE cluster for container orchestration.
  Includes node pools, RBAC, and workload identity support.

* **`gke-kafka/`**
  Extension module to deploy **Kafka on top of GKE**, enabling event streaming inside the cluster.

### 🔹 **Kafka**

* **`kafka/`**
  Standalone Kafka cluster deployment.
  Can be used independently or paired with `database` for **CQRS/event sourcing** workloads.

### 🔹 **Networking**

* **`networking/`**
  Handles VPC, subnets, and firewall rules.
  Designed as a foundation for all other modules.

---

## ⚙️ Pipelines

The `pipelines/` folder contains automation for running Terraform in CI/CD:

* **`jobs/terraform-infra-gcp.yml`** → Deploys infrastructure (main pipeline).
* **`jobs/terraform-publish-gcp.yml`** → Publishes infra modules (if used as registry).

---

## 🚀 Usage

### Example: Deploy GKE + Database

```bash
terraform init
terraform apply -var="env=dev" -var="enable_gke=true" -var="enable_database=true"
```

### Example: Deploy Kafka + Networking only

```bash
terraform init
terraform apply -var="env=prod" -var="enable_kafka=true" -var="enable_networking=true"
```

---

## 🔮 Why this structure?

✔ Modular → Each service (GKE, Kafka, DB) is isolated and reusable.
✔ Composable → Combine modules (e.g., GKE + Kafka) without rewriting.
✔ DevOps-ready → Pipelines for CI/CD baked in.
✔ Cloud-native → Uses official GCP Terraform providers.

