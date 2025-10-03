resource "google_storage_bucket" "terraform_state" {
  name          = "my-terraform-state-bucket"  # 🏗️ Change this
  location      = "US"
  storage_class = "STANDARD"

  versioning {
    enabled = true
  }

  lifecycle_rule {
    condition {
      age = 365
    }
    action {
      type = "Delete"
    }
  }
}

# ✅ Bucket Lock for State Locking (Alternative to DynamoDB)
resource "google_storage_bucket_iam_binding" "terraform_state_lock" {
  bucket = google_storage_bucket.terraform_state.name
  role   = "roles/storage.admin"

  members = [
    "serviceAccount:terraform-state@your-gcp-project.iam.gserviceaccount.com"
  ]
}
