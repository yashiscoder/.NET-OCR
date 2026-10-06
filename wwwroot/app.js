(() => {
  const fileInput = document.querySelector("#fileInput");
  const dropZone = document.querySelector("#dropZone");
  const selectedFile = document.querySelector("#selectedFile");
  const fileName = document.querySelector("#fileName");
  const analyzeButton = document.querySelector("#analyzeButton");
  const buttonLabel = document.querySelector("#buttonLabel");
  const statusMessage = document.querySelector("#statusMessage");
  const results = document.querySelector("#results");
  const maxFileSize = 5 * 1024 * 1024;
  let currentFile = null;

  function setStatus(message = "") {
    statusMessage.textContent = message;
  }

  function selectFile(file) {
    setStatus();
    if (!file) return;

    const extension = file.name.split(".").pop()?.toLowerCase();
    if (!["jpg", "jpeg", "png"].includes(extension)) {
      setStatus("Choose a JPG, JPEG, or PNG image.");
      return;
    }
    if (file.size > maxFileSize) {
      setStatus("This image is larger than 5 MB. Choose a smaller file.");
      return;
    }
    if (file.size === 0) {
      setStatus("That file is empty. Choose another image.");
      return;
    }

    currentFile = file;
    fileName.textContent = file.name;
    selectedFile.hidden = false;
    analyzeButton.disabled = false;
    results.hidden = true;
  }

  function clearFile() {
    currentFile = null;
    fileInput.value = "";
    selectedFile.hidden = true;
    analyzeButton.disabled = true;
  }

  function showResults(data) {
    document.querySelector("#jsonOutput").textContent = JSON.stringify(data, null, 2);
    results.hidden = false;
    results.scrollIntoView({ behavior: "smooth", block: "start" });
  }

  document.querySelector("#browseButton").addEventListener("click", () => fileInput.click());
  fileInput.addEventListener("change", () => selectFile(fileInput.files?.[0]));
  document.querySelector("#removeFile").addEventListener("click", clearFile);
  document.querySelector("#copyJson").addEventListener("click", async () => {
    const button = document.querySelector("#copyJson");
    try {
      await navigator.clipboard.writeText(document.querySelector("#jsonOutput").textContent);
      button.textContent = "Copied";
      window.setTimeout(() => { button.textContent = "Copy JSON"; }, 1500);
    } catch {
      setStatus("Could not copy JSON. Select it directly from the result box.");
    }
  });
  document.querySelector("#resetButton").addEventListener("click", () => {
    clearFile();
    results.hidden = true;
    setStatus();
    window.scrollTo({ top: 0, behavior: "smooth" });
  });

  for (const eventName of ["dragenter", "dragover"]) {
    dropZone.addEventListener(eventName, (event) => {
      event.preventDefault();
      dropZone.classList.add("dragging");
    });
  }
  for (const eventName of ["dragleave", "drop"]) {
    dropZone.addEventListener(eventName, (event) => {
      event.preventDefault();
      dropZone.classList.remove("dragging");
    });
  }
  dropZone.addEventListener("drop", (event) => selectFile(event.dataTransfer?.files?.[0]));

  analyzeButton.addEventListener("click", async () => {
    if (!currentFile) return;
    setStatus();
    analyzeButton.disabled = true;
    analyzeButton.classList.add("loading");
    buttonLabel.textContent = "Reading your marksheet…";

    try {
      const form = new FormData();
      form.append("file", currentFile);
      const response = await fetch("/api/marksheet", { method: "POST", body: form });
      if (!response.ok) {
        const detail = await response.text();
        throw new Error(detail || `Request failed (${response.status}).`);
      }
      showResults(await response.json());
    } catch (error) {
      const message = error instanceof TypeError
        ? "Could not reach the server. Check your connection and try again."
        : (error.message || "We couldn’t read this image. Please try another one.");
      setStatus(message.replace(/<[^>]*>/g, "").slice(0, 220));
    } finally {
      analyzeButton.disabled = !currentFile;
      analyzeButton.classList.remove("loading");
      buttonLabel.textContent = "Read this marksheet";
    }
  });
})();
