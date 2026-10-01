(function () {
    'use strict';

    var dadosEl = document.getElementById('dados-visao');
    if (!dadosEl) {
        return;
    }

    var visao = JSON.parse(dadosEl.textContent || 'null');
    if (!visao || !Array.isArray(visao.series) || visao.series.length === 0) {
        return;
    }

    var container = document.getElementById('grafico-visao');
    var legend = document.getElementById('legenda-visao');

    function fmtData(iso) {
        var p = (iso || '').split('-');
        if (p.length < 3) {
            return '';
        }
        return String(Number(p[2])) + '/' + String(Number(p[1])) + '/' + p[0];
    }

    function fmtNumero(v) {
        return Math.round(v).toLocaleString('pt-BR');
    }

    function vazio() {
        container.innerHTML = '<div class="grafico-vazio">Sem dados no período.</div>';
        legend.innerHTML = '';
    }

    function legenda(series) {
        var html = '<div class="d-flex flex-wrap gap-3">';
        series.forEach(function (s) {
            var valor = s.pontos.reduce(function (a, p) { return a + p.valor; }, 0);
            html += '<span class="small">' +
                '<span class="me-1 d-inline-block" style="width:0.75rem;height:0.75rem;background:' + s.cor + ';border-radius:0.2rem"></span>' +
                s.nome + ' <span class="text-muted">' + fmtNumero(valor) + '</span></span>';
        });
        html += '</div>';
        legend.innerHTML = html;
    }

    function renderizarColunas(linha) {
        legend.innerHTML = '';

        var porData = {};
        visao.series.forEach(function (s, si) {
            s.pontos.forEach(function (p) {
                var chave = p.data;
                if (!porData[chave]) {
                    porData[chave] = { data: chave, rotulo: p.rotulo || fmtData(chave), valores: [] };
                }
                porData[chave].valores.push({ serie: si, valor: p.valor });
            });
        });

        var datas = Object.keys(porData).sort();
        if (datas.length === 0) {
            vazio();
            return;
        }

        var max = 0;
        datas.forEach(function (d) {
            porData[d].valores.forEach(function (v) {
                if (v.valor > max) {
                    max = v.valor;
                }
            });
        });
        if (max <= 0) {
            vazio();
            return;
        }

        var W = 900, H = 340, padL = 8, padR = 8, padT = 10, padB = 20;
        var areaW = W - padL - padR;
        var areaH = H - padT - padB;
        var y = function (v) { return padT + areaH - (v / max) * areaH; };

        var svg = '<svg viewBox="0 0 ' + W + ' ' + H + '" xmlns="http://www.w3.org/2000/svg" preserveAspectRatio="xMidYMid meet" role="img">';
        svg += '<line x1="' + padL + '" y1="' + y(0) + '" x2="' + (W - padR) + '" y2="' + y(0) + '" stroke="#dee2e6" stroke-width="1"/>';

        var porDataW = areaW / datas.length;
        var nSeries = visao.series.length;

        if (linha) {
            visao.series.forEach(function (s, si) {
                var pontos = datas.map(function (d, i) {
                    var encontrado = porData[d].valores.find(function (v) { return v.serie === si; });
                    var x = datas.length > 1 ? padL + (i / (datas.length - 1)) * areaW : padL + areaW / 2;
                    return { x: x, y: y(encontrado ? encontrado.valor : 0) };
                });
                var area = pontos.map(function (pt) { return pt.x + ',' + pt.y; }).join(' ');
                var linhaSvg = pontos.map(function (pt) { return pt.x + ',' + pt.y; }).join(' ');
                svg += '<polygon points="' + area + ' ' + (W - padR) + ',' + y(0) + ' ' + padL + ',' + y(0) + '" fill="' + s.cor + '" opacity="0.10"/>';
                svg += '<polyline points="' + linhaSvg + '" fill="none" stroke="' + s.cor + '" stroke-width="2" stroke-linejoin="round"/>';
            });
        } else {
            var porSerieW = (porDataW * 0.86) / nSeries;
            visao.series.forEach(function (s, si) {
                datas.forEach(function (d, i) {
                    var encontrado = porData[d].valores.find(function (v) { return v.serie === si; });
                    var valor = encontrado ? encontrado.valor : 0;
                    var x0 = padL + i * porDataW + porDataW * 0.07 + si * porSerieW;
                    var h = valor > 0 ? Math.max((valor / max) * areaH, 1) : 0;
                    if (h > 0) {
                        svg += '<rect x="' + x0 + '" y="' + y(valor) + '" width="' + porSerieW + '" height="' + h + '" fill="' + s.cor + '" rx="1.5"/>';
                    }
                });
            });
        }

        svg += '</svg>';
        container.innerHTML = svg;
        legenda(visao.series);
    }

    function renderizarDonut() {
        var total = visao.valorTotal || visao.series.reduce(function (a, s) {
            return a + s.pontos.reduce(function (b, p) { return b + p.valor; }, 0);
        }, 0);
        if (total <= 0) {
            vazio();
            return;
        }

        var cx = 160, cy = 160, r = 120, sw = 42;
        var C = 2 * Math.PI * r;
        var offset = 0;

        var svg = '<svg viewBox="0 0 320 320" xmlns="http://www.w3.org/2000/svg" preserveAspectRatio="xMidYMid meet" role="img">';
        visao.series.forEach(function (s) {
            var valor = s.pontos.reduce(function (a, p) { return a + p.valor; }, 0);
            var len = (valor / total) * C;
            var gap = visao.series.length > 1 ? 3 : 0;
            var dash = Math.max(len - gap, 0.5);
            svg += '<circle cx="' + cx + '" cy="' + cy + '" r="' + r + '" fill="none" stroke="' + s.cor + '" stroke-width="' + sw + '" ' +
                'stroke-dasharray="' + dash + ' ' + (C - dash) + '" stroke-dashoffset="' + (-offset) + '" transform="rotate(-90 ' + cx + ' ' + cy + ')"/>';
            offset += len;
        });
        svg += '<text x="' + cx + '" y="' + (cy - 4) + '" text-anchor="middle" font-size="26" font-weight="600" fill="#212529">' + fmtNumero(total) + '</text>';
        svg += '<text x="' + cx + '" y="' + (cy + 18) + '" text-anchor="middle" font-size="12" fill="#6c757d">' + visao.unidade + '</text>';
        svg += '</svg>';

        container.innerHTML = svg;
        legenda(visao.series);
    }

    if (visao.forma === 'donut') {
        renderizarDonut();
    } else {
        renderizarColunas(visao.forma === 'linha');
    }
})();