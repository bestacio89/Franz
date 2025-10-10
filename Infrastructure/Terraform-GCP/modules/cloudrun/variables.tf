variable "gcp_region" {
  description = "GCP region for Cloud Run service"
  type        = string
}

variable "microservice_name" {
  description = "Name of the Cloud Run microservice"
  type        = string
}

variable "container_image" {
  description = "Container image to deploy"
  type        = string
}

variable "cpu_limit" {
  description = "CPU limit for the container (e.g. 1000m = 1 vCPU)"
  type        = string
  default     = "1000m"
}

variable "memory_limit" {
  description = "Memory limit for the container (e.g. 512Mi)"
  type        = string
  default     = "512Mi"
}

variable "container_port" {
  description = "Port the container listens on"
  type        = number
  default     = 8080
}

variable "allow_unauthenticated" {
  description = "Whether to allow unauthenticated access to the service"
  type        = bool
  default     = true
}
