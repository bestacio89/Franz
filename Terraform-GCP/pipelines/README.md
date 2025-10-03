Perfect ✅ — here’s a **README.md** tailored for your `pipelines/` folder in **Terraform-GCP**. It explains the two pipelines (`terraform-infra-gcp.yml` and `terraform-publish-gcp.yml`) in a clean, developer/DevOps-friendly way.

---

# 🚀 Terraform GCP Pipelines

This folder contains Azure DevOps pipeline definitions for managing **Google Cloud Platform (GCP) infrastructure** and **microservice deployments**.
The pipelines are designed to be **manual-trigger only** (`trigger: none`), giving teams full control over when infra changes or deployments are executed.

---

## 📂 Pipelines Overview

### 1️⃣ `terraform-infra-gcp.yml`

**Purpose:** Provision and manage **infrastructure** on GCP using Terraform.

🔧 **Parameters**

* `databaseType` → `single` | `multi`
* `eventStorageType` → `Kafka` | `Firestore` | `MongoDB` *(only for multi-DB)*
* `entityStorageType` → `PostgreSQL` | `MariaDB` | `MySQL` | `SQLServer` | `MongoDB`

🔄 **Stages**

1. **Terraform Init** → Initializes backend and providers.
2. **Terraform Validate** → Validates Terraform code and modules.
3. **Terraform Plan** → Shows planned changes based on selected parameters.
4. **Terraform Apply** → Applies infra changes to GCP.

🔐 **Secrets**

* `GCP_PROJECT_ID`
* `GCP_SERVICE_ACCOUNT_KEY`

---

### 2️⃣ `terraform-publish-gcp.yml`

**Purpose:** Build, push, and deploy **microservices** into GCP (GKE or Cloud Run).

🔧 **Parameters**

* `repository` → Source repository name (default: `Franz`).
* `serviceName` → Microservice name (default: `microservice`).
* `gcpArtifactRegistry` → Artifact Registry URL for Docker images.
* `deployToGKE` → `true` → deploys to **GKE**, `false` → deploys to **Cloud Run**.

🔄 **Stages**

1. **Build & Push Docker** →

   * Authenticates to Artifact Registry.
   * Builds and pushes the Docker image.
2. **Deploy Microservice** →

   * If `deployToGKE=true` → Deploys via `kubectl set image`.
   * If `deployToGKE=false` → Deploys via `gcloud run deploy`.

🔐 **Secrets**

* `GCP_PROJECT_ID`
* `GCP_REGION` (default: `us-central1`)
* `GCP_SERVICE_ACCOUNT_KEY`

---

## 🛠 Usage Workflow

1. Run **`terraform-infra-gcp.yml`** to provision or update infrastructure.
   Example: deploy a multi-DB setup with Kafka.

2. Run **`terraform-publish-gcp.yml`** to build and deploy your microservice.
   Choose GKE or Cloud Run at runtime with the `deployToGKE` parameter.

---

## ✅ Why this setup?

* Separation of concerns: **infra** vs **service deployments**.
* Fully parameterized for **multi-environment** flexibility (dev, staging, prod).
* Cloud-native workflow with **Terraform + Docker + GCP** integration.
* Manual trigger only → no accidental infra changes or deployments.


