terraform {
  backend "s3" {
    bucket         = var.tf_backend_bucket             # S3 bucket for remote state
    key            = "terraform/${terraform.workspace}/terraform.tfstate"
    region         = var.tf_backend_region
    encrypt        = true
    dynamodb_table = var.tf_backend_lock_table         # DynamoDB table for state locking
  }

  required_version = ">= 1.6.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}
