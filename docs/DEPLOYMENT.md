# Deployment Guide

## ☸️ Kubernetes Deployment

This project uses **Kustomize** to manage environment-specific configurations (Secrets, NFS IPs) and **Ingress** to route traffic.

### Prerequisites (Raspberry Pi / K3s)
1.  **K3s**: Ensure K3s is installed.
    *   *Note:* **Traefik** (Ingress Controller) is enabled by default. Verify with `kubectl get pods -n kube-system | grep traefik`.
2.  **NFS Server**: You need an NFS server for persistent storage.
3.  **Docker Registry Secret**: You must manually create the secret for pulling images from GHCR:
    ```bash
    kubectl create secret docker-registry ghcr-secret \
      --docker-server=ghcr.io \
      --docker-username=YOUR_GITHUB_USER \
      --docker-password=YOUR_GITHUB_TOKEN \
      --docker-email=YOUR_EMAIL
    ```

### 🔐 Managing App Secrets
We use a **SecretGenerator** strategy for application secrets (DB passwords, etc.).
*   **Old Way**: You manually ran `kubectl create secret generic whiskey-secrets`.
*   **New Way**: The CI pipeline generates a versioned secret (e.g., `whiskey-secrets-h5k2`) automatically on every deploy.
    *   *Benefit*: Changing a secret in GitHub and deploying will automatically restart your pods with the new values.
    *   *Note*: Your old manual `whiskey-secrets` will be ignored by the new deployment. You can delete it if you wish.

**Required Secrets (GitHub):**
*   `SECRETS_ENV_FILE`: The full content of the `.env` file.
    ```env
    POSTGRES_USER=postgres
    POSTGRES_PASSWORD=your_secure_password
    # Add other env vars needed by the app container
    ```

### 🔑 Setting up GitHub Secrets

To make the CI/CD pipeline work, you need to add the following secrets in your GitHub Repository:
1.  Go to **Settings** > **Secrets and variables** > **Actions**.
2.  Click **New repository secret**.
3.  Add the following:

| Name | Value | Description |
| :--- | :--- | :--- |
| `PI_HOST` | `192.168.x.x` (or public IP) | The IP address of your Raspberry Pi. |
| `PI_USER` | `pi` (or your user) | The SSH username for the Pi. |
| `PI_SSH_KEY` | `-----BEGIN OPENSSH PRIVATE KEY...` | The private SSH key to access the Pi. |
| `SECRETS_ENV_FILE` | (See Content Below) | The exact content of the `.env` file to be created on the Pi. |

**Content for `SECRETS_ENV_FILE`:**
```env
# Database Credentials
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_super_secret_password
POSTGRES_DB=whiskey_tracker

# Connection String for App (Must match DB creds)
# Note: Use the Service Name 'postgres-service' as the host
Database__ConnectionString=Host=postgres-service;Database=whiskey_tracker;Username=postgres;Password=your_super_secret_password
Database__Provider=Postgres
```

Database__Provider=Postgres
```

#### 🔑 Generating the SSH Key
If you don't have an SSH key pair for GitHub Actions yet, generate one on your local machine:

1.  **Generate the Key Pair:**
    ```bash
    ssh-keygen -t ed25519 -C "github-actions-whiskey" -f ./whiskey_key
    ```
    *   Press Enter for no passphrase (automation needs passwordless access).

2.  **Install Public Key on Pi:**
    *   Copy the content of `whiskey_key.pub`.
    *   SSH into your Pi (`ssh pi@192.168.x.x`).
    *   Add the content to `~/.ssh/authorized_keys`.
    *   *Tip:* You can use `ssh-copy-id -i ./whiskey_key.pub pi@192.168.x.x` if available.

3.  **Add Private Key to GitHub:**
    *   Copy the **entire content** of the file `whiskey_key` (the one without extension).
    *   Paste this into the **PI_SSH_KEY** secret in GitHub.
    *   **Security Note:** Delete the local private key (`rm whiskey_key`) after pasting it.

### 🌍 Networking (Ingress)
*   **Service**: `whiskey-service` runs as `ClusterIP` (internal only).
*   **Ingress**: Routes `http://whiskey-tracker.local` -> `whiskey-service:80`.
*   **Access**:
    *   Add `192.168.x.x whiskey-tracker.local` to your local machine's `/etc/hosts` file (where `192.168.x.x` is your Pi's IP).
    *   **Nginx Proxy Manager**: Point a Proxy Host for `yourdomain.com` to `192.168.x.x` (Port 80). Traefik will handle the rest.

### 💾 Storage (NFS)
The `k8s/storage.yaml` file is generic.
The actual NFS Server IP is injected via `k8s/patches/nfs-server.yaml`.

**To change the NFS IP:**
1.  Edit `k8s/patches/nfs-server.yaml`.
2.  Commit and Push.

### 🚀 Manual Deployment
If you need to deploy manually from the Pi:

1.  Create `k8s/.env` with your secrets.
2.  Run:
    ```bash
    kubectl apply -k k8s/
    ```
