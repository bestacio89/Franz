# ⚙️ Jobs for Terraform GCP Pipelines

This folder contains **reusable job templates** used by the pipelines under [`pipelines/`](../).
Each job encapsulates a specific step in either **infrastructure provisioning** (Terraform) or **service deployment** (Docker, GKE, Cloud Run).

---

## 📂 Job Templates

### 🌍 Terraform Jobs

* **`terraform-init.yml`**
  Initializes Terraform with backend configuration and providers.

* **`terraform-validate.yml`**
  Validates Terraform configuration and modules before planning.

* **`terraform-plan.yml`**
  Runs `terraform plan` to show the infrastructure changes that would be applied.
  Accepts parameters:

  * `databaseType`
  * `eventStorageType`
  * `entityStorageType`

* **`terraform-apply.yml`**
  Runs `terraform apply` to actually provision/update GCP infrastructure.

---

### 🐳 Docker & Deployment Jobs

* **`docker-build.yml`**
  Builds and pushes a Docker image to **GCP Artifact Registry**.

* **`deploy-gke.yml`**
  Deploys the built image to a **GKE cluster** via `kubectl`.
  Ensures rollout completion with `kubectl rollout status`.

* **`deploy-cloudrun.yml`**
  Deploys the built image to **Cloud Run** via `gcloud run deploy`.
  Supports fully managed serverless workloads.

---

## ✅ How They Fit Together

* **Infrastructure pipeline** (`terraform-infra-gcp.yml`)
  → Uses `terraform-init.yml`, `terraform-validate.yml`, `terraform-plan.yml`, `terraform-apply.yml`.

* **Service publish pipeline** (`terraform-publish-gcp.yml`)
  → Uses `docker-build.yml`, then either `deploy-gke.yml` **or** `deploy-cloudrun.yml`.

---

## 🔑 Benefits of this structure

* **Reusability** → same job definitions can be plugged into different pipelines.
* **Separation of concerns** → Terraform vs Docker vs Deployment jobs are clearly split.
* **Portability** → jobs can be reused in other projects with minimal changes.

---

👉 With this structure, your **pipelines act as orchestrators** and your **jobs folder provides the building blocks**.

