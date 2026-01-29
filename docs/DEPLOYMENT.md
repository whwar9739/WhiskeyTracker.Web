# Deployment Guide

## ☸️ Kubernetes Deployment

This project uses **Kustomize** to manage environment-specific configurations (Secrets, NFS IPs) and **NodePort** services for external access.

### Prerequisites (Raspberry Pi / K3s/ MicroK8s)
1.  **Kubernetes Cluster**: Ensure your cluster is running.
2.  **NFS Server**: You need an NFS server for persistent storage (photos).
3.  **Docker Registry Secret**: You must manually create the secret for pulling images from GHCR:
    ```bash
    kubectl create secret docker-registry ghcr-secret \
      --docker-server=ghcr.io \
      --docker-username=YOUR_GITHUB_USER \
      --docker-password=YOUR_GITHUB_TOKEN \
      --docker-email=YOUR_EMAIL
    ```

### 🔐 Managing App Secrets
We use a **SecretGenerator** strategy for application secrets.
*   The CI pipeline generates a versioned secret (e.g., `whiskey-secrets-h5k2`) automatically on every deploy.
*   This triggers an automatic pod rollout when secrets change.

**Required Secrets (GitHub):**
You must configure the `SECRETS_ENV_FILE` secret in GitHub with the following content:

```env
# Database Credentials
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_super_secret_password
POSTGRES_DB=whiskey_prod

# Connection String (Used by both App and Data Protection)
# IMPORTANT: 'host=postgres-service' refers to the internal K8s Service name for the DB
connection-string=Host=postgres-service;Database=whiskey_prod;Username=postgres;Password=your_super_secret_password

# Authentication (Google)
Authentication__Google__ClientId=your_google_client_id
Authentication__Google__ClientSecret=your_google_client_secret

# Email
EmailSettings__Host=smtp.gmail.com
EmailSettings__Port=587
EmailSettings__User=your_email@gmail.com
EmailSettings__Password=your_app_password
```

### 🌍 Networking (NodePort)
We use a **NodePort** service to expose the application on a static port, simplifying the integration with external proxies like Nginx Proxy Manager.

*   **Service Type**: `NodePort`
*   **External Port**: `30080`
*   **Internal Port**: `8080`

#### Accessing from Outside
You typically put an **Nginx Proxy Manager (NPM)** or another reverse proxy in front of the cluster.

**Nginx Proxy Manager Configuration:**
1.  **Domain Names**: `whiskeytracker.ferrinhouse.org`
2.  **Scheme**: `http` (The cluster listens on HTTP)
3.  **Forward Hostname / IP**: `192.168.x.x` (IP of your Kubernetes Node)
4.  **Forward Port**: `30080`
5.  **SSL Tab**:
    *   **Force SSL**: Enabled
    *   **Certificate**: Valid Let's Encrypt Certificate

**Note on Google Authentication:**
The application automatically forces the request scheme to `https` when it detects it is behind a proxy (`X-Forwarded-Host` present). This ensures that Google OAuth redirects work correctly even though NPM terminates SSL.

### 💾 Storage (NFS)
The `k8s/storage.yaml` file is generic.
The actual NFS Server IP is injected via `k8s/patches/nfs-server.yaml`.

**To change the NFS IP:**
1.  Edit `k8s/patches/nfs-server.yaml`.
2.  Commit and Push.

### 🚀 Manual Deployment
If you need to deploy manually from the Pi:

1.  Create `k8s/.env` with your secrets (see above).
2.  Run:
    ```bash
    kubectl apply -k k8s/
    ```
    *Note: This will deploy the app, database, and storage configurations.*
