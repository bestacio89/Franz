variable "gcp_region" {}
variable "kafka_cluster_name" {}

resource "google_container_cluster" "kafka_gke" {
  name     = "kafka-gke-cluster"
  location = var.gcp_region

  node_pool {
    name       = "default-pool"
    node_count = 3

    node_config {
      machine_type = "e2-standard-2"
      disk_size_gb = 50
      oauth_scopes = [
        "https://www.googleapis.com/auth/cloud-platform"
      ]
    }
  }
}

resource "kubernetes_deployment" "kafka" {
  metadata {
    name      = var.kafka_cluster_name
    namespace = "default"
  }

  spec {
    replicas = 3

    selector {
      match_labels = {
        app = var.kafka_cluster_name
      }
    }

    template {
      metadata {
        labels = {
          app = var.kafka_cluster_name
        }
      }

      spec {
        container {
          name  = "kafka"
          image = "bitnami/kafka:latest"
          ports {
            container_port = 9092
          }
          env {
            name  = "KAFKA_ZOOKEEPER_CONNECT"
            value = "zookeeper:2181"
          }
          env {
            name  = "KAFKA_ADVERTISED_LISTENERS"
            value = "PLAINTEXT://kafka-service.default.svc.cluster.local:9092"
          }
          resources {
            limits = {
              cpu    = "1000m"
              memory = "1Gi"
            }
          }
        }
      }
    }
  }
}

resource "kubernetes_service" "kafka_service" {
  metadata {
    name      = "kafka-service"
    namespace = "default"
  }

  spec {
    selector = {
      app = var.kafka_cluster_name
    }

    port {
      protocol    = "TCP"
      port        = 9092
      target_port = 9092
    }

    type = "LoadBalancer"
  }
}
