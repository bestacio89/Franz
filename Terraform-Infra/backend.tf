terraform {
  backend "s3" {
    bucket         = "my-terraform-state-bucket"  # 🏗️ Change to your actual S3 bucket
    key            = "terraform/state.tfstate"   # Path inside the bucket
    region         = "us-east-1"
    encrypt        = true
    dynamodb_table = "terraform-lock-table"  # 🔒 DynamoDB for state locking
  }
}
