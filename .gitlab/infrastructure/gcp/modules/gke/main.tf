variable "gcp_region" {}
variable "microservice_name" {}
variable "container_image" {}

resource "google_container_cluster" "microservice_gke" {
  name     = "gke-microservice-cluster"
  location = var.gcp_region

  node_pool {
    name       = "default-pool"
    node_count = 2

    node_config {
      machine_type = "e2-medium"
      disk_size_gb = 20
      oauth_scopes = [
        "https://www.googleapis.com/auth/cloud-platform"
      ]
    }
  }
}

resource "kubernetes_deployment" "microservice_deployment" {
  metadata {
    name      = var.microservice_name
    namespace = "default"
  }

  spec {
    replicas = 2

    selector {
      match_labels = {
        app = var.microservice_name
      }
    }

    template {
      metadata {
        labels = {
          app = var.microservice_name
        }
      }

      spec {
        container {
          name  = var.microservice_name
          image = var.container_image
          ports {
            container_port = 8080
          }
          resources {
            limits = {
              cpu    = "500m"
              memory = "256Mi"
            }
          }
        }
      }
    }
  }
}

resource "kubernetes_service" "microservice_service" {
  metadata {
    name      = var.microservice_name
    namespace = "default"
  }

  spec {
    selector = {
      app = var.microservice_name
    }

    port {
      protocol    = "TCP"
      port        = 80
      target_port = 8080
    }

    type = "LoadBalancer"
  }
}
