// DOM Elements
const uploadArea = document.getElementById('uploadArea');
const fileInput = document.getElementById('fileInput');
const fileName = document.getElementById('fileName');
const password = document.getElementById('password');
const togglePassword = document.getElementById('togglePassword');
const decryptForm = document.getElementById('decryptForm');
const decryptButton = document.getElementById('decryptButton');
const message = document.getElementById('message');

let selectedFile = null;

// File upload via click
uploadArea.addEventListener('click', e => {
	if (e.target !== fileInput) {
		fileInput.click();
	}
});

fileInput.addEventListener('change', e => {
	handleFileSelect(e.target.files[0]);
});

// Drag and drop functionality
uploadArea.addEventListener('dragover', e => {
	e.preventDefault();
	uploadArea.classList.add('drag-over');
});

uploadArea.addEventListener('dragleave', () => {
	uploadArea.classList.remove('drag-over');
});

uploadArea.addEventListener('drop', e => {
	e.preventDefault();
	uploadArea.classList.remove('drag-over');

	const files = e.dataTransfer.files;
	if (files.length > 0) {
		handleFileSelect(files[0]);
	}
});

// Handle file selection
function handleFileSelect(file) {
	if (!file) return;

	// Validate file type
	const validExtensions = ['.xlsx', '.xls'];
	const fileExtension = '.' + file.name.split('.').pop().toLowerCase();

	if (!validExtensions.includes(fileExtension)) {
		showMessage('Please select a valid Excel file (.xlsx or .xls)', 'error');
		return;
	}

	selectedFile = file;
	fileName.textContent = `📄 ${file.name}`;
	hideMessage();
}

// Toggle password visibility
togglePassword.addEventListener('click', () => {
	const type = password.type === 'password' ? 'text' : 'password';
	password.type = type;
	togglePassword.textContent = type === 'password' ? '👁️' : '🙈';
});

// Form submission
decryptForm.addEventListener('submit', async e => {
	e.preventDefault();

	if (!selectedFile) {
		showMessage('Please select a file to decrypt', 'error');
		return;
	}

	if (!password.value.trim()) {
		showMessage('Please enter the decryption password', 'error');
		password.focus();
		return;
	}

	await decryptFile();
});

// Decrypt file function
async function decryptFile() {
	const formData = new FormData();
	formData.append('file', selectedFile);
	formData.append('password', password.value);

	// Show loading state
	decryptButton.classList.add('loading');
	decryptButton.disabled = true;
	hideMessage();

	try {
		const response = await fetch('/decrypt-excel/decrypt', {
			method: 'POST',
			body: formData,
		});

		if (response.ok) {
			// Get the decrypted file
			const blob = await response.blob();
			const contentDisposition = response.headers.get('Content-Disposition');
			let downloadFileName = 'decrypted_file.xlsx';

			// Extract filename from Content-Disposition header if available
			if (contentDisposition) {
				const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(
					contentDisposition
				);
				if (matches != null && matches[1]) {
					downloadFileName = matches[1].replace(/['"]/g, '');
				}
			}

			// Create download link
			const url = window.URL.createObjectURL(blob);
			const a = document.createElement('a');
			a.href = url;
			a.download = downloadFileName;
			document.body.appendChild(a);
			a.click();
			window.URL.revokeObjectURL(url);
			document.body.removeChild(a);

			showMessage(
				'✅ File decrypted successfully! Download started.',
				'success'
			);

			// Reset form
			setTimeout(() => {
				resetForm();
			}, 2000);
		} else {
			// Handle error response
			const errorData = await response.json();
			showMessage(`❌ ${errorData.error || 'Decryption failed'}`, 'error');
		}
	} catch (error) {
		console.error('Error:', error);
		showMessage(
			'❌ An error occurred while decrypting the file. Please try again.',
			'error'
		);
	} finally {
		// Hide loading state
		decryptButton.classList.remove('loading');
		decryptButton.disabled = false;
	}
}

// Show message
function showMessage(text, type) {
	message.textContent = text;
	message.className = `message show ${type}`;
}

// Hide message
function hideMessage() {
	message.classList.remove('show');
}

// Reset form
function resetForm() {
	selectedFile = null;
	fileName.textContent = '';
	password.value = '';
	fileInput.value = '';
	hideMessage();
}

// Allow Enter key to submit when password field is focused
password.addEventListener('keypress', e => {
	if (e.key === 'Enter') {
		decryptForm.dispatchEvent(new Event('submit'));
	}
});
