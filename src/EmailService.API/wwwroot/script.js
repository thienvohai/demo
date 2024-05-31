const globalPageSize = 10;
changePage(1);
function populateTable(emails) {
    const tableBody = document.getElementById('emailLists');
    tableBody.innerHTML = '';

    emails.forEach(email => {
        const row = document.createElement('tr');

        const idCell = document.createElement('td');
        idCell.textContent = email.id;
        const statusCell = document.createElement('td');
        statusCell.textContent = email.status;
        const retryCountCell = document.createElement('td');
        retryCountCell.textContent = email.retryCount;
        const isRetryingCell = document.createElement('td');
        isRetryingCell.textContent = email.isRetrying;
        const senderCell = document.createElement('td');
        senderCell.textContent = email.sender;
        const senderNameCell = document.createElement('td');
        senderNameCell.textContent = email.senderName;
        const toCell = document.createElement('td');
        toCell.textContent = email.to;
        const ccCell = document.createElement('td');
        ccCell.textContent = email.cc;
        const bccCell = document.createElement('td');
        bccCell.textContent = email.bcc;
        const subjectCell = document.createElement('td');
        subjectCell.textContent = email.subject;
        const bodyCell = document.createElement('td');
        const maxLength = 50;
        bodyCell.textContent = email.body.length > maxLength ? email.body.substring(0, maxLength) + '...' : email.body;
        const attachmentCell = document.createElement('td');
        attachmentCell.textContent = email.attachment.length > maxLength ? email.attachment.substring(0, maxLength) + '...' : email.attachment;

        row.appendChild(idCell);
        row.appendChild(statusCell);
        row.appendChild(retryCountCell);
        row.appendChild(isRetryingCell);
        row.appendChild(senderCell);
        row.appendChild(senderNameCell);
        row.appendChild(toCell);
        row.appendChild(ccCell);
        row.appendChild(bccCell);
        row.appendChild(subjectCell);
        row.appendChild(bodyCell);
        row.appendChild(attachmentCell);

        tableBody.appendChild(row);
    });
}

function generatePagination(totalPages) {
    const paginationElement = document.getElementById('pagination');
    paginationElement.innerHTML = ''; // Clear existing pagination links

    for (let i = 1; i <= totalPages; i++) {
        const link = `<a href="#" onclick="changePage(${i})">${i}</a>`;
        paginationElement.innerHTML += link;
    }
}

function changePage(pageNumber) {
    fetch(`/api/v1/admin/emails`, {
        method: `POST`,
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ pageSize: globalPageSize, pageIndex: pageNumber })
    })
    .then(response => response.json())
    .then(data => {
        if (data.isError) {
            alert("Error when getting emails data. Message:" + data.message + " .Log Id: " + data.correlatedId)
            return;
        }
        let totalPage = 0;
        data.result.total % globalPageSize !== 0 ? totalPage = data.result.total / globalPageSize + 1 : data.result.total / globalPageSize
        generatePagination(totalPage);
        populateTable(data.result.emails);
        highlightActivePage(pageNumber);
    })
    .catch(error => console.error('Error fetching user data:', error));
}

function highlightActivePage(pageNumber) {
    const paginationLinks = document.querySelectorAll('.pagination a');
    paginationLinks.forEach(link => {
        link.classList.remove('active');
        if (Number(link.innerText) === pageNumber) {
            link.classList.add('active');
        }
    });
}