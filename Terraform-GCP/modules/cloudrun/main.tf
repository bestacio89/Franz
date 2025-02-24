variable "gcp_region" {}
variable "microservice_name" {}
variable "container_image" {}

resource "google_cloud_run_service" "microservice" {
  name     = var.microservice_name
  location = var.gcp_region

  template {
    spec {
      containers {
        image = var.container_image
        ports {
          container_port = 8080
        }
        resources {
          limits = {
            cpu    = "1000m"
            memory = "512Mi"
          }
        }
      }
    }
  }

  traffic {
    percent         = 100
    latest_revision = true
  }
}

resource "google_cloud_run_service_iam_policy" "allow_unauthenticated" {
  location = google_cloud_run_service.microservice.location
  service  = google_cloud_run_service.microservice.name

  policy_data = <<EOT
{
  "bindings": [
    {
      "role": "roles/run.invoker",
      "members": ["allUsers"]
    }
  ]
}
EOT
}
