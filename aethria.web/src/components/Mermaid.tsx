import React, { useEffect, useRef, useState } from "react";
import mermaid from "mermaid";

interface MermaidProps {
  chart: string;
}

export const Mermaid: React.FC<MermaidProps> = ({ chart }) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const [svg, setSvg] = useState<string>("");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!chart) return;

    // Detect dark mode from the document element attribute
    const isDark =
      document.documentElement.getAttribute("data-mantine-color-scheme") ===
      "dark";

    mermaid.initialize({
      startOnLoad: false,
      theme: isDark ? "dark" : "default",
      securityLevel: "loose",
      themeVariables: {
        background: isDark ? "#1a1b1e" : "#ffffff",
        primaryColor: isDark ? "#25262b" : "#f8f9fa",
        lineColor: isDark ? "#5c5f66" : "#dee2e6",
      },
    });

    const uniqueId = `mermaid-${Math.random().toString(36).substring(2, 9)}`;

    const renderChart = async () => {
      try {
        setError(null);
        const { svg: renderedSvg } = await mermaid.render(uniqueId, chart);
        setSvg(renderedSvg);
      } catch (err: unknown) {
        console.error("Mermaid render error:", err);
        const errorMessage = err instanceof Error ? err.message : String(err);
        setError(errorMessage || "Error rendering diagram");

        // Remove dirty elements left over by Mermaid render failure
        const element = document.getElementById(uniqueId);
        if (element) {
          element.remove();
        }
      }
    };

    renderChart();
  }, [chart]);

  if (error) {
    return (
      <div
        style={{
          padding: "1rem",
          border: "1px solid #f87171",
          borderRadius: "8px",
          backgroundColor: "rgba(239, 68, 68, 0.05)",
          color: "#ef4444",
          fontFamily: "monospace",
          fontSize: "0.85rem",
        }}
      >
        <div style={{ fontWeight: "bold", marginBottom: "0.5rem" }}>
          Failed to render visual diagram:
        </div>
        <pre
          style={{ margin: 0, whiteSpace: "pre-wrap", wordBreak: "break-all" }}
        >
          {error}
        </pre>
      </div>
    );
  }

  return (
    <div
      ref={containerRef}
      style={{
        display: "flex",
        justifyContent: "center",
        width: "100%",
        overflowX: "auto",
        padding: "1rem",
        borderRadius: "8px",
        border: "1px solid var(--mantine-color-default-border)",
        backgroundColor: "var(--mantine-color-body)",
      }}
      dangerouslySetInnerHTML={{ __html: svg }}
    />
  );
};
