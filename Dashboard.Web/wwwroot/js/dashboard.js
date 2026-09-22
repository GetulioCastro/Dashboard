(function () {
    'use strict';

    var dadosEl = document.getElementById('dados-indicadores');
    if (!dadosEl) {
        return;
    }

    var indicadores = JSON.parse(dadosEl.textContent || '[]');
    var estado = {
        inicio: '',
        fim: '',
        maximizado: ''
    };

    var paleta = ['#0d6efd', '#198754', '#0dcaf0', '#ffc107', '#dc3545', '#6f42c1', '#fd7e14'];

    function elemento(id) {
        return document.getElementById(id);
    }

    function hoje() {
        var d = new Date();
        return { ano: d.getFullYear(), mes: d.getMonth(), dia: d.getDate() };
    }

    function isoLocal(d) {
        var mes = String(d.getMonth() + 1).padStart(2, '0');
        var dia = String(d.getDate()).padStart(2, '0');
        return d.getFullYear() + '-' + mes + '-' + dia;
    }

    function parseISO(iso) {
        var p = iso.split('-');
        return new Date(+p[0], +p[1] - 1, +p[2]);
    }

    function fmtData(iso) {
        var d = parseISO(iso);
        return String(d.getDate()).padStart(2, '0') + '/' + String(d.getMonth() + 1).padStart(2, '0') + '/' + d.getFullYear();
    }

    function fmtNumero(v) {
        return Math.round(v).toLocaleString('pt-BR');
    }

    function fmtMoeda(v) {
        return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', minimumFractionDigits: 2 });
    }

    function fmtValor(v, monetario) {
        return monetario ? fmtMoeda(v) : fmtNumero(v);
    }

    function corPara(id) {
        for (var i = 0; i < indicadores.length; i++) {
            if (indicadores[i].id === id) {
                return paleta[i % paleta.length];
            }
        }
        return paleta[0];
    }

    function definirMesAtual() {
        var h = hoje();
        estado.inicio = h.ano + '-' + String(h.mes + 1).padStart(2, '0') + '-01';
        estado.fim = isoLocal(new Date());
    }

    function definirUltimosDias(dias) {
        var d = new Date();
        d.setDate(d.getDate() - (dias - 1));
        estado.inicio = isoLocal(d);
        estado.fim = isoLocal(new Date());
    }

    function definirAnoAtual() {
        var h = hoje();
        estado.inicio = h.ano + '-01-01';
        estado.fim = isoLocal(new Date());
    }

    function diasEntre(inicio, fim) {
        return Math.round((parseISO(fim) - parseISO(inicio)) / 86400000);
    }

    function semanaInicio(iso) {
        var d = parseISO(iso);
        var desloc = (d.getDay() + 6) % 7;
        d.setDate(d.getDate() - desloc);
        return d;
    }

    function agrupar(pontos) {
        var dif = diasEntre(estado.inicio, estado.fim);
        var chave, rotulo;
        var agrupados = {};

        pontos.forEach(function (p) {
            var d = parseISO(p.data);
            if (dif <= 35) {
                chave = p.data;
                rotulo = String(d.getDate()).padStart(2, '0') + '/' + String(d.getMonth() + 1).padStart(2, '0');
            } else if (dif <= 130) {
                var s = semanaInicio(p.data);
                chave = isoLocal(s);
                rotulo = 'Sem ' + String(s.getDate()).padStart(2, '0') + '/' + String(s.getMonth() + 1).padStart(2, '0');
            } else {
                chave = p.data.slice(0, 7);
                rotulo = d.toLocaleDateString('pt-BR', { month: 'short', year: 'numeric' });
            }

            if (!agrupados[chave]) {
                agrupados[chave] = { rotulo: rotulo, valor: 0 };
            }
            agrupados[chave].valor += p.valor;
        });

        return Object.keys(agrupados).map(function (k) { return agrupados[k]; });
    }

    function pontosFiltrados(ind) {
        return ind.pontos.filter(function (p) {
            return p.data >= estado.inicio && p.data <= estado.fim;
        });
    }

    function renderizarSvg(container, grupos, forma, cor, largura, altura) {
        var W = largura || 220;
        var H = altura || 170;
        var padS = 4;
        var padB = 18;
        var padT = 6;
        var max = 0;

        grupos.forEach(function (g) {
            if (g.valor > max) {
                max = g.valor;
            }
        });

        if (grupos.length === 0 || max <= 0) {
            container.innerHTML = '<div class="grafico-vazio">Nenhum dado encontrado</div>';
            return;
        }

        var areaW = W - padS * 2;
        var areaH = H - padT - padB;
        var porCol = areaW / grupos.length;
        var y = function (v) { return padT + areaH - (v / max) * areaH; };

        var svg = '<svg viewBox="0 0 ' + W + ' ' + H + '" xmlns="http://www.w3.org/2000/svg" preserveAspectRatio="xMidYMid meet" role="img">';
        svg += '<line x1="' + padS + '" y1="' + y(0) + '" x2="' + (W - padS) + '" y2="' + y(0) + '" stroke="#dee2e6" stroke-width="1"/>';

        if (largura >= 420) {
            svg += '<text x="' + padS + '" y="' + (y(0) - 4) + '" font-size="9" fill="#adb5bd">0</text>';
            svg += '<text x="' + padS + '" y="' + (y(max) + 10) + '" font-size="9" fill="#adb5bd">max</text>';
        }

        if (forma === 'coluna') {
            grupos.forEach(function (g, i) {
                var x0 = padS + i * porCol + porCol * 0.15;
                var larg = porCol * 0.7;
                var h = Math.max((g.valor / max) * areaH, 1);
                svg += '<rect x="' + x0 + '" y="' + y(g.valor) + '" width="' + larg + '" height="' + h + '" fill="' + cor + '" rx="1.5"/>';
            });
        } else {
            var pontosLinha = '';
            grupos.forEach(function (g, i) {
                var x0 = grupos.length > 1 ? padS + (i / (grupos.length - 1)) * areaW : padS + areaW / 2;
                pontosLinha += x0 + ',' + y(g.valor) + ' ';
                if (i === grupos.length - 1) {
                    svg += '<circle cx="' + x0 + '" cy="' + y(g.valor) + '" r="2.5" fill="' + cor + '"/>';
                }
            });
            var area = grupos.map(function (g, i) {
                var x0 = grupos.length > 1 ? padS + (i / (grupos.length - 1)) * areaW : padS + areaW / 2;
                return x0 + ',' + y(g.valor);
            }).join(' ');
            svg += '<polygon points="' + area + ' ' + (grupos.length > 1 ? (W - padS) + ',' + y(0) : (padS + areaW / 2) + ',' + y(0)) + ' ' + padS + ',' + y(0) + '" fill="' + cor + '" opacity="0.12"/>';
            svg += '<polyline points="' + pontosLinha.trim() + '" fill="none" stroke="' + cor + '" stroke-width="2" stroke-linejoin="round"/>';
        }

        svg += '</svg>';
        container.innerHTML = svg;
    }

    function renderizarTudo() {
        elemento('status-periodo').textContent = 'Período: ' + fmtData(estado.inicio) + ' a ' + fmtData(estado.fim);
        elemento('filtro-info').textContent = 'Dados demonstrativos referentes à data atual.';

        indicadores.forEach(function (ind) {
            var pontos = pontosFiltrados(ind);
            var total = pontos.reduce(function (s, p) { return s + p.valor; }, 0);
            var elValor = document.querySelector('[data-valor="' + ind.id + '"]');
            var elCaption = document.querySelector('[data-referencia-caption="' + ind.id + '"]');
            var container = document.querySelector('[data-grafico="' + ind.id + '"]');

            elValor.textContent = pontos.length > 0 ? fmtValor(total, ind.monetario) : '—';
            elCaption.textContent = 'Período: ' + fmtData(estado.inicio) + ' a ' + fmtData(estado.fim);

            renderizarSvg(container, agrupar(pontos), ind.forma, corPara(ind.id), 220, 170);
        });
    }

    function atualizarSelecao() {
        var n = 0;
        document.querySelectorAll('.chk-selecionar').forEach(function (chk) {
            var id = chk.id.replace('sel-', '');
            var card = elemento('card-' + id);
            if (chk.checked) {
                card.classList.add('selecionado');
                n++;
            } else {
                card.classList.remove('selecionado');
            }
        });
        elemento('qtd-selecionados').textContent = String(n);
    }

    function aplicarFiltro() {
        var a = elemento('filtro-inicio').value;
        var b = elemento('filtro-fim').value;
        var alerta = elemento('filtro-alerta');

        if (!a || !b) {
            alerta.textContent = 'Informe a Data Inicial e a Data Final.';
            alerta.classList.remove('d-none');
            return;
        }

        if (a > b) {
            alerta.textContent = 'A Data Inicial não pode ser maior que a Data Final.';
            alerta.classList.remove('d-none');
            return;
        }

        alerta.classList.add('d-none');
        estado.inicio = a;
        estado.fim = b;
        renderizarTudo();
    }

    elemento('btn-aplicar').addEventListener('click', aplicarFiltro);

    elemento('filtro-inicio').addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            aplicarFiltro();
        }
    });

    elemento('filtro-fim').addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            aplicarFiltro();
        }
    });

    document.querySelectorAll('[data-periodo]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var tipo = btn.getAttribute('data-periodo');
            if (tipo === 'mes') {
                definirMesAtual();
            } else if (tipo === '90d') {
                definirUltimosDias(90);
            } else {
                definirAnoAtual();
            }
            elemento('filtro-inicio').value = estado.inicio;
            elemento('filtro-fim').value = estado.fim;
            renderizarTudo();
        });
    });

    document.querySelectorAll('.chk-selecionar').forEach(function (chk) {
        chk.addEventListener('change', atualizarSelecao);
    });

    document.querySelectorAll('[data-menu-indicador]').forEach(function (link) {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            var id = link.getAttribute('data-menu-indicador');
            var card = elemento('card-' + id);
            if (!card) {
                return;
            }

            card.scrollIntoView({ behavior: 'smooth', block: 'center' });
            card.classList.remove('destaque');
            void card.offsetWidth;
            card.classList.add('destaque');

            var chk = elemento('sel-' + id);
            if (chk && !chk.checked) {
                chk.checked = true;
                atualizarSelecao();
            }

            document.querySelectorAll('[data-menu-indicador]').forEach(function (x) {
                x.classList.remove('active');
            });
            link.classList.add('active');
        });
    });

    var modalEl = elemento('modal-maximizado');

    document.querySelectorAll('.btn-maximizar').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var id = btn.getAttribute('data-indicador');
            var ind = indicadores.find(function (x) { return x.id === id; });
            if (!ind) {
                return;
            }

            estado.maximizado = id;
            elemento('modal-titulo').textContent = ind.nome;
            elemento('modal-unidade').textContent = ind.unidade;
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
        });
    });

    modalEl.addEventListener('shown.bs.modal', function () {
        var ind = indicadores.find(function (x) { return x.id === estado.maximizado; });
        if (!ind) {
            return;
        }

        var pontos = pontosFiltrados(ind);
        var total = pontos.reduce(function (s, p) { return s + p.valor; }, 0);

        elemento('modal-valor').textContent = pontos.length > 0 ? fmtValor(total, ind.monetario) : '—';
        elemento('modal-periodo').textContent = 'Período: ' + fmtData(estado.inicio) + ' a ' + fmtData(estado.fim);

        renderizarSvg(elemento('modal-grafico'), agrupar(pontos), ind.forma, corPara(ind.id), 900, 380);
    });

    definirMesAtual();
    elemento('filtro-inicio').value = estado.inicio;
    elemento('filtro-fim').value = estado.fim;
    renderizarTudo();
})();