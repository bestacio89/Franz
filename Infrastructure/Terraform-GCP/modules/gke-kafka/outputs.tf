output "kafka_gke_service_ip" {
  value = kubernetes_service.kafka_service.status[0].load_balancer[0].ingress[0].ip
}
variable "gcp_region" {
  description = "GCP region where the GKE cluster will be deployed"
  type        = string
}

variable "kafka_cluster_name" {
  description = "The name to assign to the Kafka deployment and service"
  type        = string
  default     = "kafka"
}

variable "node_count" {
  description = "Number of nodes in the GKE node pool"
  type        = number
  default     = 3
}

variable "machine_type" {
  description = "Machine type for GKE worker nodes"
  type        = string
  default     = "e2-standard-2"
}

variable "disk_size_gb" {
  description = "Disk size (in GB) for GKE worker nodes"
  type        = number
  default     = 50
}
      