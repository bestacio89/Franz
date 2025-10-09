variable "gcp_region" {
  description = "GCP region where the GKE cluster will be deployed"
  type        = string
}

variable "microservice_name" {
  description = "The name of the microservice (used for deployment and service)"
  type        = string
}

variable "container_image" {
  description = "Container image for the microservice (e.g., gcr.io/project/image:tag)"
  type        = string
}

variable "replicas" {
  description = "Number of replicas for the microservice deployment"
  type        = number
  default     = 2
}

variable "container_port" {
  description = "Port exposed inside the container"
  type        = number
  default     = 8080
}

variable "service_port" {
  description = "Port exposed by the Kubernetes Service"
  type        = number
  default     = 80
}

variable "env_vars" {
  description = "Environment variables for the microservice container"
  type        = map(string)
  default     = {}
}

variable "secrets" {
  description = "Kubernetes secrets to mount as environment variables (secret_name=key)"
  type        = map(string)
  default     = {}
}
