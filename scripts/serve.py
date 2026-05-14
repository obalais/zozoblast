#!/usr/bin/env python3
"""Tiny HTTP server for the local Unity WebGL build.

Serves Builds/WebGL/ over LAN so the game can be tested from a phone on the
same WiFi. Handles Brotli (.br) Content-Encoding for compressed builds, but
the project currently builds uncompressed for iOS Safari compatibility.

Usage:
    python3 scripts/serve.py [port]   # defaults to 8000
"""
import http.server
import os
import sys


class UnityWebGLHandler(http.server.SimpleHTTPRequestHandler):
    def guess_type(self, path):
        if path.endswith(".br"):
            return super().guess_type(path[:-3])
        return super().guess_type(path)

    def end_headers(self):
        if self.path.endswith(".br"):
            self.send_header("Content-Encoding", "br")
        super().end_headers()


def main() -> None:
    port = int(sys.argv[1]) if len(sys.argv) > 1 else 8000
    project_root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    build_dir = os.path.join(project_root, "Builds", "WebGL")
    if not os.path.isdir(build_dir):
        sys.exit(f"Build directory not found: {build_dir}\nBuild the WebGL target in Unity first.")
    os.chdir(build_dir)

    with http.server.ThreadingHTTPServer(("0.0.0.0", port), UnityWebGLHandler) as httpd:
        print(f"Serving {build_dir} on http://0.0.0.0:{port}")
        print(f"Open on this Mac:     http://127.0.0.1:{port}")
        print(f"Open on phone (WiFi): http://<your-lan-ip>:{port}")
        httpd.serve_forever()


if __name__ == "__main__":
    main()
