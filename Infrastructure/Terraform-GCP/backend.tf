resource "google_storage_bucket" "terraform_state" {
  name          = "${var.project_id}-terraform-state"  # ✅ Unique & project-scoped
  location      = var.region                           # ✅ Parametrized
  storage_class = "STANDARD"

  versioning {
    enabled = true   # ✅ Protects against state corruption
  }

  uniform_bucket_level_access = true   # ✅ Security best practice

  lifecycle_rule {
    condition {
      age = 365
    }
    action {
      type = "Delete"
    }
  }

  labels = {
    environment = var.environment
    managed_by  = "terraform"
    purpose     = "state-backend"
  }
}

# ✅ Locking & Access Control (IAM Binding)
resource "google_storage_bucket_iam_binding" "terraform_state_lock" {
  bucket = google_storage_bucket.terraform_state.name
  role   = "roles/storage.objectAdmin"

  members = [
    "serviceAccount:${var.terraform_sa_email}" # ✅ Parametrized service account
  ]
}
